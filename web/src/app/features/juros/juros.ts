import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { format } from 'date-fns';
import { finalize } from 'rxjs';
import { JurosApi } from '../../core/apis';
import { aplicarErrosDaApi, ehErroDeValidacao } from '../../core/erros';
import { normalizarValor, valorMonetario } from '../../core/formatos';
import { ResultadoJuros } from '../../core/modelos';

@Component({
  selector: 'app-juros',
  imports: [
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './juros.html',
  styleUrl: './juros.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Juros {
  private readonly api = inject(JurosApi);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly resultado = signal<ResultadoJuros | null>(null);
  protected readonly calculando = signal(false);

  protected readonly form = this.fb.group({
    valor: this.fb.control('', [Validators.required, valorMonetario]),
    vencimento: new FormControl<Date | null>(null, Validators.required),
  });

  protected calcular(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { valor, vencimento } = this.form.getRawValue();
    this.calculando.set(true);

    // format() do date-fns usa a data local. toISOString() converteria para UTC e,
    // em fusos a leste de Greenwich, a meia-noite local viraria o dia anterior.
    this.api
      .calcular(normalizarValor(valor)!, format(vencimento!, 'yyyy-MM-dd'))
      .pipe(finalize(() => this.calculando.set(false)))
      .subscribe({
        next: (r) => this.resultado.set(r),
        error: (e: HttpErrorResponse) => ehErroDeValidacao(e) && aplicarErrosDaApi(this.form, e),
      });
  }
}
