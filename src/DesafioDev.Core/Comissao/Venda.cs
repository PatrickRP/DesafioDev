namespace DesafioDev.Core.Comissao;

public sealed record Venda
{
    public string Vendedor { get; }
    public decimal Valor { get; }

    public Venda(string vendedor, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(vendedor))
            throw new ArgumentException("Vendedor é obrigatório.", nameof(vendedor));
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "Valor da venda não pode ser negativo.");

        Vendedor = vendedor.Trim();
        Valor = valor;
    }
}

/// <summary>Formato do arquivo de origem: { "vendas": [ ... ] }.</summary>
public sealed record ArquivoVendas(IReadOnlyList<Venda> Vendas);
