import { MatPaginatorIntl } from '@angular/material/paginator';
import { Routes } from '@angular/router';
import { PaginadorPtBr } from '../../core/formatos';
import { Estoque } from './estoque';

export default [
  {
    path: '',
    title: 'Estoque | Desafio Dev',
    component: Estoque,
    providers: [{ provide: MatPaginatorIntl, useClass: PaginadorPtBr }],
  },
] satisfies Routes;
