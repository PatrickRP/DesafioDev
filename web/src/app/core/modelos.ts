// Espelham os records da API (System.Text.Json serializa em camelCase).
// decimal do C# chega como number: aqui só exibimos, nenhum cálculo monetário é refeito no front.

export interface ComissaoVenda {
  valor: number;
  percentual: number;
  comissao: number;
}

export interface ComissaoVendedor {
  vendedor: string;
  quantidadeVendas: number;
  vendasComissionadas: number;
  totalVendido: number;
  totalComissao: number;
  vendas: ComissaoVenda[];
}

export interface ComissoesResponse {
  vendedores: ComissaoVendedor[];
  quantidadeVendas: number;
  totalVendido: number;
  totalComissao: number;
}

export type TipoMovimentacao = 'Entrada' | 'Saida';

export interface Produto {
  codigo: number;
  descricao: string;
  quantidade: number;
}

export interface Movimentacao {
  id: number;
  codigoProduto: number;
  tipo: TipoMovimentacao;
  quantidade: number;
  descricao: string;
  dataHora: string;
  estoqueAnterior: number;
  estoqueFinal: number;
}

export interface NovaMovimentacao {
  codigoProduto: number;
  tipo: TipoMovimentacao;
  quantidade: number;
  descricao: string;
}

export interface ResultadoJuros {
  valorOriginal: number;
  vencimento: string; // DateOnly → "yyyy-MM-dd"
  dataCalculo: string;
  diasAtraso: number;
  taxaDiaria: number;
  juros: number;
  valorAtualizado: number;
}

/** RFC 9457, formato de erro de todos os endpoints. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
