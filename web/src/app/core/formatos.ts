import { AbstractControl, ValidationErrors } from '@angular/forms';
import { MatDateFormats } from '@angular/material/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { Injectable } from '@angular/core';

/**
 * Converte o que o usuário digitou em decimal invariante ("1234.56"), ou null se inválido.
 * Com vírgula → formato brasileiro (1.234,56); sem vírgula → ponto é decimal (1234.56).
 * Devolve string, não number: o valor vai na query string sem passar por ponto flutuante.
 */
export function normalizarValor(texto: string | null | undefined): string | null {
  if (!texto) return null;
  let s = texto.replace(/R\$|\s/g, '');
  if (s.includes(',')) s = s.replace(/\./g, '').replace(',', '.');
  if (!/^\d+(\.\d{1,2})?$/.test(s)) return null;
  return Number(s) > 0 ? s : null;
}

export function valorMonetario(c: AbstractControl<string>): ValidationErrors | null {
  return !c.value || normalizarValor(c.value) ? null : { valorMonetario: true };
}

export function inteiro(c: AbstractControl<number | null>): ValidationErrors | null {
  return c.value == null || Number.isInteger(c.value) ? null : { inteiro: true };
}

export function semSoEspacos(c: AbstractControl<string>): ValidationErrors | null {
  return c.value && !c.value.trim() ? { required: true } : null;
}

/** O parse aceita a data digitada com ou sem barras; a exibição é sempre dd/MM/yyyy. */
export const FORMATOS_DATA: MatDateFormats = {
  parse: { dateInput: ['dd/MM/yyyy', 'd/M/yyyy', 'dd/MM/yy', 'ddMMyyyy'] },
  display: {
    dateInput: 'dd/MM/yyyy',
    monthYearLabel: 'MMM yyyy',
    dateA11yLabel: 'PPP',
    monthYearA11yLabel: 'MMMM yyyy',
  },
};

@Injectable()
export class PaginadorPtBr extends MatPaginatorIntl {
  override itemsPerPageLabel = 'Por página';
  override nextPageLabel = 'Próxima página';
  override previousPageLabel = 'Página anterior';
  override firstPageLabel = 'Primeira página';
  override lastPageLabel = 'Última página';
  override getRangeLabel = (pagina: number, tamanho: number, total: number) => {
    if (total === 0) return '0 de 0';
    const inicio = pagina * tamanho;
    return `${inicio + 1}–${Math.min(inicio + tamanho, total)} de ${total}`;
  };
}
