import { CurrencyPipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { ComissoesApi } from '../../core/apis';
import { ComissaoVendedor, ComissoesResponse } from '../../core/modelos';

@Component({
  selector: 'app-comissoes',
  imports: [
    CurrencyPipe,
    PercentPipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './comissoes.html',
  styleUrl: './comissoes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Comissoes {
  private readonly api = inject(ComissoesApi);

  protected readonly dados = signal<ComissoesResponse | null>(null);
  protected readonly carregando = signal(true);
  protected readonly falhou = signal(false);
  protected readonly aberto = signal<string | null>(null);

  protected readonly colunas = ['vendedor', 'vendas', 'totalVendido', 'totalComissao', 'expandir'];

  /** Quanto da venda total virou comissão: dá a leitura rápida do mix de faixas. */
  protected readonly taxaEfetiva = computed(() => {
    const d = this.dados();
    return d && d.totalVendido > 0 ? d.totalComissao / d.totalVendido : 0;
  });

  constructor() {
    this.api
      .listar()
      .pipe(
        takeUntilDestroyed(),
        finalize(() => this.carregando.set(false)),
      )
      .subscribe({
        next: (d) => this.dados.set(d),
        error: () => this.falhou.set(true),
      });
  }

  protected alternar(v: ComissaoVendedor): void {
    this.aberto.update((atual) => (atual === v.vendedor ? null : v.vendedor));
  }
}
