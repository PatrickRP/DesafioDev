import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'comissoes' },
  {
    path: 'comissoes',
    title: 'Comissões | Desafio Dev',
    loadComponent: () => import('./features/comissoes/comissoes').then((m) => m.Comissoes),
  },
  // loadChildren (e não loadComponent) porque estas rotas têm providers próprios: declarados no
  // arquivo lazy, as dependências deles também só baixam quando a rota é visitada.
  { path: 'estoque', loadChildren: () => import('./features/estoque/estoque.routes') },
  { path: 'juros', loadChildren: () => import('./features/juros/juros.routes') },
  { path: '**', redirectTo: 'comissoes' },
];
