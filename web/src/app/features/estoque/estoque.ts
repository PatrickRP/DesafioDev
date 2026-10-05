import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroupDirective,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize, merge } from 'rxjs';
import { EstoqueApi } from '../../core/apis';
import { aplicarErrosDaApi, ehErroDeValidacao } from '../../core/erros';
import { inteiro, semSoEspacos } from '../../core/formatos';
import { Movimentacao, Produto, TipoMovimentacao } from '../../core/modelos';

@Component({
  selector: 'app-estoque',
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './estoque.html',
  styleUrl: './estoque.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Estoque {
  private readonly api = inject(EstoqueApi);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly produtos = signal<Produto[]>([]);
  protected readonly movimentacoes = signal<Movimentacao[]>([]);
  protected readonly ultima = signal<Movimentacao | null>(null);
  protected readonly enviando = signal(false);

  protected readonly form = this.fb.group({
    codigoProduto: new FormControl<number | null>(null, Validators.required),
    tipo: this.fb.control<TipoMovimentacao>('Entrada'),
    quantidade: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(1),
      inteiro,
      (c: AbstractControl) => this.saldoSuficiente(c),
    ]),
    descricao: this.fb.control('', [Validators.required, Validators.maxLength(200), semSoEspacos]),
  });

  private readonly codigoSelecionado = toSignal(this.form.controls.codigoProduto.valueChanges, {
    initialValue: null,
  });
  protected readonly tipoSelecionado = toSignal(this.form.controls.tipo.valueChanges, {
    initialValue: this.form.controls.tipo.value,
  });
  protected readonly produtoSelecionado = computed(
    () => this.produtos().find((p) => p.codigo === this.codigoSelecionado()) ?? null,
  );

  // ---- histórico ----
  protected readonly filtro = signal<number | null>(null);
  protected readonly pagina = signal(0);
  protected readonly tamanhoPagina = signal(10);
  protected readonly paginaAtual = computed(() => {
    const inicio = this.pagina() * this.tamanhoPagina();
    return this.movimentacoes().slice(inicio, inicio + this.tamanhoPagina());
  });
  protected readonly colunasHistorico = [
    'id',
    'dataHora',
    'produto',
    'tipo',
    'quantidade',
    'saldo',
    'descricao',
  ];
  protected readonly colunasProdutos = ['codigo', 'descricao', 'quantidade'];

  private readonly nomes = computed(
    () => new Map(this.produtos().map((p) => [p.codigo, p.descricao])),
  );

  constructor() {
    this.carregarProdutos();
    this.carregarHistorico();

    // A regra de saldo depende de produto e tipo: quando eles mudam, a quantidade precisa ser revalidada.
    const { codigoProduto, tipo, quantidade } = this.form.controls;
    merge(codigoProduto.valueChanges, tipo.valueChanges)
      .pipe(takeUntilDestroyed())
      .subscribe(() => quantidade.updateValueAndValidity());
  }

  protected nomeDoProduto(codigo: number): string {
    return this.nomes().get(codigo) ?? `Produto ${codigo}`;
  }

  protected selecionar(p: Produto): void {
    this.form.controls.codigoProduto.setValue(p.codigo);
  }

  protected filtrar(codigo: number | null): void {
    this.filtro.set(codigo);
    this.carregarHistorico();
  }

  protected mudarPagina(e: PageEvent): void {
    this.pagina.set(e.pageIndex);
    this.tamanhoPagina.set(e.pageSize);
  }

  protected registrar(formDir: FormGroupDirective): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { codigoProduto, tipo, quantidade, descricao } = this.form.getRawValue();
    this.enviando.set(true);

    this.api
      .movimentar({
        codigoProduto: codigoProduto!,
        tipo,
        quantidade: quantidade!,
        descricao: descricao.trim(),
      })
      .pipe(finalize(() => this.enviando.set(false)))
      .subscribe({
        next: (mov) => {
          this.ultima.set(mov);
          // O saldo da resposta é o do banco no momento da gravação: atualiza localmente sem novo GET.
          this.produtos.update((ps) =>
            ps.map((p) =>
              p.codigo === mov.codigoProduto ? { ...p, quantidade: mov.estoqueFinal } : p,
            ),
          );
          this.carregarHistorico();
          // resetForm (e não form.reset) limpa também o estado "submitted" que pinta os erros.
          formDir.resetForm({ codigoProduto, tipo, quantidade: null, descricao: '' });
        },
        error: (e: HttpErrorResponse) => {
          if (ehErroDeValidacao(e)) {
            aplicarErrosDaApi(this.form, e);
          } else if (e.status === 422 || e.status === 409) {
            // Outra pessoa movimentou o mesmo produto: o saldo exibido estava velho.
            this.carregarProdutos();
          }
        },
      });
  }

  /** Espelho da regra do servidor só para feedback imediato; quem decide continua sendo a API. */
  private saldoSuficiente(c: AbstractControl<number | null>): ValidationErrors | null {
    const grupo = c.parent;
    if (!grupo || c.value == null || grupo.get('tipo')?.value !== 'Saida') return null;

    const produto = this.produtos().find((p) => p.codigo === grupo.get('codigoProduto')?.value);
    return produto && c.value > produto.quantidade
      ? { saldoInsuficiente: { disponivel: produto.quantidade } }
      : null;
  }

  private carregarProdutos(): void {
    this.api.produtos().subscribe((ps) => {
      this.produtos.set(ps);
      this.form.controls.quantidade.updateValueAndValidity();
    });
  }

  private carregarHistorico(): void {
    this.api.movimentacoes(this.filtro()).subscribe((ms) => {
      this.movimentacoes.set(ms);
      this.pagina.set(0);
    });
  }
}
