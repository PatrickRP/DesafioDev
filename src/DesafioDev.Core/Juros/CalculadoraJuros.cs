namespace DesafioDev.Core.Juros;

public sealed record ResultadoJuros(
    decimal ValorOriginal,
    DateOnly Vencimento,
    DateOnly DataCalculo,
    int DiasAtraso,
    decimal TaxaDiaria,
    decimal Juros,
    decimal ValorAtualizado);

/// <summary>
/// Juros simples por dia corrido de atraso: juros = valor × taxa × dias.
/// Pagamento até o vencimento (inclusive) não gera encargo.
/// </summary>
public sealed class CalculadoraJuros
{
    public const decimal TaxaDiariaPadrao = 0.025m; // 2,5% ao dia

    private readonly TimeProvider _relogio;
    private readonly decimal _taxaDiaria;

    public CalculadoraJuros(TimeProvider? relogio = null, decimal taxaDiaria = TaxaDiariaPadrao)
    {
        if (taxaDiaria < 0) throw new ArgumentOutOfRangeException(nameof(taxaDiaria));
        _relogio = relogio ?? TimeProvider.System;
        _taxaDiaria = taxaDiaria;
    }

    public ResultadoJuros Calcular(decimal valor, DateOnly vencimento)
    {
        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "Valor deve ser maior que zero.");

        var hoje = DateOnly.FromDateTime(_relogio.GetLocalNow().DateTime);
        var dias = Math.Max(0, hoje.DayNumber - vencimento.DayNumber);

        var juros = Math.Round(valor * _taxaDiaria * dias, 2, MidpointRounding.AwayFromZero);

        return new ResultadoJuros(valor, vencimento, hoje, dias, _taxaDiaria, juros, valor + juros);
    }
}
