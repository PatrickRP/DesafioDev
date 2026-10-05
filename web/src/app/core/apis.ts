import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  ComissoesResponse,
  Movimentacao,
  NovaMovimentacao,
  Produto,
  ResultadoJuros,
} from './modelos';

// URLs relativas: em dev o proxy do `ng serve` encaminha /api para a API (proxy.conf.json),
// então o browser vê uma origem só e não há CORS para configurar.
const API = '/api';

@Injectable({ providedIn: 'root' })
export class ComissoesApi {
  private readonly http = inject(HttpClient);

  listar() {
    return this.http.get<ComissoesResponse>(`${API}/comissoes`);
  }
}

@Injectable({ providedIn: 'root' })
export class EstoqueApi {
  private readonly http = inject(HttpClient);

  produtos() {
    return this.http.get<Produto[]>(`${API}/estoque/produtos`);
  }

  movimentacoes(codigoProduto?: number | null) {
    const params = codigoProduto ? new HttpParams().set('codigoProduto', codigoProduto) : undefined;
    return this.http.get<Movimentacao[]>(`${API}/estoque/movimentacoes`, { params });
  }

  movimentar(mov: NovaMovimentacao) {
    return this.http.post<Movimentacao>(`${API}/estoque/movimentacoes`, mov);
  }
}

@Injectable({ providedIn: 'root' })
export class JurosApi {
  private readonly http = inject(HttpClient);

  /** valor já normalizado ("1234.56") e vencimento em ISO ("2026-09-25"). */
  calcular(valor: string, vencimento: string) {
    const params = new HttpParams().set('valor', valor).set('vencimento', vencimento);
    return this.http.get<ResultadoJuros>(`${API}/juros`, { params });
  }
}
