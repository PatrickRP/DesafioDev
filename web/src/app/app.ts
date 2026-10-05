import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatTabsModule, MatIconModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly secoes = [
    { rota: '/comissoes', rotulo: 'Comissões', icone: 'payments' },
    { rota: '/estoque', rotulo: 'Estoque', icone: 'inventory_2' },
    { rota: '/juros', rotulo: 'Juros', icone: 'event_busy' },
  ];
}
