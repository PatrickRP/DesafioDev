using DesafioDev.Core.Juros;

namespace DesafioDev.Tests;

public class JurosTests
{
    private static readonly CalculadoraJuros Calc = new(new RelogioFixo(2026, 10, 5));

    [Fact]
    public void Dez_dias_de_atraso_geram_25_por_cento()
    {
        var r = Calc.Calcular(1000m, new DateOnly(2026, 9, 25));

        Assert.Equal(10, r.DiasAtraso);
        Assert.Equal(250m, r.Juros);
        Assert.Equal(1250m, r.ValorAtualizado);
    }

    [Fact]
    public void Atraso_atravessando_virada_de_mes()
    {
        var r = Calc.Calcular(500m, new DateOnly(2026, 8, 31)); // 35 dias

        Assert.Equal(35, r.DiasAtraso);
        Assert.Equal(437.50m, r.Juros);
    }

    [Theory]
    [InlineData(2026, 10, 5)]  // vence hoje
    [InlineData(2026, 12, 1)]  // a vencer
    public void Sem_atraso_nao_ha_juros(int a, int m, int d)
    {
        var r = Calc.Calcular(1000m, new DateOnly(a, m, d));

        Assert.Equal(0, r.DiasAtraso);
        Assert.Equal(0m, r.Juros);
    }

    [Fact]
    public void Arredonda_juros_em_centavos()
    {
        var r = Calc.Calcular(123.45m, new DateOnly(2026, 10, 2)); // 3 dias → 9,25875

        Assert.Equal(9.26m, r.Juros);
    }

    [Fact]
    public void Valor_nao_positivo_e_invalido() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Calc.Calcular(0m, new DateOnly(2026, 1, 1)));
}
