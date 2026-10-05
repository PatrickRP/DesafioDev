import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from './modelos';

function problema(e: HttpErrorResponse): ProblemDetails | null {
  return e.error && typeof e.error === 'object' ? (e.error as ProblemDetails) : null;
}

/** 400 com `errors` por campo: quem trata é o formulário, que mostra o erro no próprio campo. */
export function ehErroDeValidacao(e: unknown): boolean {
  return e instanceof HttpErrorResponse && e.status === 400 && !!problema(e)?.errors;
}

export function mensagemDeErro(e: HttpErrorResponse): string {
  if (e.status === 0) return 'A API não respondeu. Confira se ela está rodando em localhost:5080.';
  const p = problema(e);
  return p?.detail ?? p?.title ?? `Erro ${e.status} ao chamar a API.`;
}

/**
 * Ponto único de feedback de erro HTTP. Validação de campo fica para o formulário;
 * todo o resto (404, 409, 422, 500, API fora do ar) vira snackbar.
 * O erro é relançado para o componente ainda poder reagir (ex.: recarregar saldos após 422).
 */
export const erroInterceptor: HttpInterceptorFn = (req, next) => {
  const snack = inject(MatSnackBar);
  return next(req).pipe(
    catchError((e: unknown) => {
      if (e instanceof HttpErrorResponse && !ehErroDeValidacao(e)) {
        snack.open(mensagemDeErro(e), 'Fechar', { duration: 6000, panelClass: 'snack-erro' });
      }
      return throwError(() => e);
    }),
  );
};

/** Aplica os erros de um ValidationProblem da API nos controles de mesmo nome. */
export function aplicarErrosDaApi(form: FormGroup, e: HttpErrorResponse): void {
  const erros = problema(e)?.errors ?? {};
  for (const [campo, mensagens] of Object.entries(erros)) {
    const controle = form.get(campo);
    controle?.setErrors({ ...controle.errors, api: mensagens[0] });
    controle?.markAsTouched();
  }
}
