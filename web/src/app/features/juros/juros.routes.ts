import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { Routes } from '@angular/router';
import { ptBR } from 'date-fns/locale';
import { FORMATOS_DATA } from '../../core/formatos';
import { Juros } from './juros';

export default [
  {
    path: '',
    title: 'Juros | Desafio Dev',
    component: Juros,
    providers: [
      // O NativeDateAdapter usa Date.parse, que lê "05/10/2026" como 10 de maio.
      // O adapter do date-fns faz o parse com formato e locale explícitos.
      provideDateFnsAdapter(FORMATOS_DATA),
      { provide: MAT_DATE_LOCALE, useValue: ptBR },
    ],
  },
] satisfies Routes;
