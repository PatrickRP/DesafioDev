using DesafioDev.Core.Comissao;

namespace DesafioDev.Tests;

public class ComissaoTests
{
    [Theory]
    [InlineData(0.00, 0.00)]
    [InlineData(99.99, 0.00)]
    [InlineData(100.00, 0.01)]   // fronteira inferior da faixa de 1%
    [InlineData(499.99, 0.01)]
    [InlineData(500.00, 0.05)]   // "a partir de R$ 500" → inclusivo
    [InlineData(1800.00, 0.05)]
    public void Percentual_respeita_as_faixas(double valor, double esperado) =>
        Assert.Equal((decimal)esperado, RegraComissao.Percentual((decimal)valor));

    [Theory]
    [InlineData(1200.50, 60.03)]  // 60,025 → arredonda pra cima
    [InlineData(480.75, 4.81)]    // 4,8075
    [InlineData(90.75, 0.00)]
    public void Comissao_por_venda_arredonda_em_centavos(double valor, double esperado) =>
        Assert.Equal((decimal)esperado, RegraComissao.Calcular((decimal)valor));

    [Fact]
    public void Calcula_totais_do_dataset_do_desafio()
    {
        var resultado = CalculadoraComissao.Calcular(DadosDesafio.Vendas())
            .ToDictionary(c => c.Vendedor, c => c.TotalComissao);

        Assert.Equal(495.69m, resultado["João Silva"]);
        Assert.Equal(465.96m, resultado["Maria Souza"]);
        Assert.Equal(379.38m, resultado["Carlos Oliveira"]);
        Assert.Equal(404.99m, resultado["Ana Lima"]);
    }

    [Fact]
    public void Agrupa_vendedor_ignorando_caixa_e_espacos()
    {
        var vendas = new[] { new Venda("Ana Lima", 1000m), new Venda(" ana lima ", 200m) };

        var r = CalculadoraComissao.Calcular(vendas);

        var unico = Assert.Single(r);
        Assert.Equal(2, unico.QuantidadeVendas);
        Assert.Equal(52m, unico.TotalComissao);
    }

    [Fact]
    public void Venda_negativa_e_rejeitada() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Venda("X", -1m));
}

internal static class DadosDesafio
{
    public static IEnumerable<Venda> Vendas()
    {
        var dados = new (string Vendedor, decimal[] Valores)[]
        {
            ("João Silva", [1200.50m, 950.75m, 1800.00m, 1400.30m, 1100.90m, 1550.00m, 1700.80m, 250.30m, 480.75m, 320.40m]),
            ("Maria Souza", [2100.40m, 1350.60m, 950.20m, 1600.75m, 1750.00m, 1450.90m, 400.50m, 180.20m, 90.75m]),
            ("Carlos Oliveira", [800.50m, 1200.00m, 1950.30m, 1750.80m, 1300.60m, 300.40m, 500.00m, 125.75m]),
            ("Ana Lima", [1000.00m, 1100.50m, 1250.75m, 1400.20m, 1550.90m, 1650.00m, 75.30m, 420.90m, 315.40m])
        };
        return dados.SelectMany(d => d.Valores.Select(v => new Venda(d.Vendedor, v)));
    }
}
