namespace DesafioDev.Core.Comissao;

public sealed record ComissaoVenda(decimal Valor, decimal Percentual, decimal Comissao);

public sealed record ComissaoVendedor(
    string Vendedor,
    int QuantidadeVendas,
    int VendasComissionadas,
    decimal TotalVendido,
    decimal TotalComissao,
    IReadOnlyList<ComissaoVenda> Vendas);

public static class CalculadoraComissao
{
    public static IReadOnlyList<ComissaoVendedor> Calcular(IEnumerable<Venda> vendas)
    {
        ArgumentNullException.ThrowIfNull(vendas);

        return vendas
            .GroupBy(v => v.Vendedor, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var itens = g
                    .Select(v => new ComissaoVenda(v.Valor, RegraComissao.Percentual(v.Valor), RegraComissao.Calcular(v.Valor)))
                    .ToList();

                return new ComissaoVendedor(
                    Vendedor: g.First().Vendedor,
                    QuantidadeVendas: itens.Count,
                    VendasComissionadas: itens.Count(i => i.Percentual > 0),
                    TotalVendido: itens.Sum(i => i.Valor),
                    // Soma das comissões já arredondadas por venda: cada venda gera seu próprio
                    // lançamento, então o total tem que bater com a soma dos lançamentos.
                    TotalComissao: itens.Sum(i => i.Comissao),
                    Vendas: itens);
            })
            .OrderByDescending(c => c.TotalComissao)
            .ThenBy(c => c.Vendedor, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
