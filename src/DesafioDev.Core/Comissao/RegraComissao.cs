namespace DesafioDev.Core.Comissao;

/// <summary>
/// Regra aplicada venda a venda (não sobre o total do vendedor):
///   valor &lt; 100        → 0%
///   100 ≤ valor &lt; 500  → 1%
///   valor ≥ 500         → 5%
/// </summary>
public static class RegraComissao
{
    public const decimal LimiteSemComissao = 100m;
    public const decimal LimiteFaixaMaior = 500m;

    public static decimal Percentual(decimal valorVenda) => valorVenda switch
    {
        < LimiteSemComissao => 0m,
        < LimiteFaixaMaior => 0.01m,
        _ => 0.05m
    };

    /// <summary>Comissão de uma única venda, arredondada a centavos.</summary>
    public static decimal Calcular(decimal valorVenda) =>
        Math.Round(valorVenda * Percentual(valorVenda), 2, MidpointRounding.AwayFromZero);
}
