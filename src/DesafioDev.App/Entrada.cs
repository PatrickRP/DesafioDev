using System.Globalization;

namespace DesafioDev.App;

/// <summary>Leitura de console tolerante a erro de digitação: repete a pergunta até receber algo válido.</summary>
internal static class Entrada
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Texto(string rotulo)
    {
        while (true)
        {
            Console.Write($"{rotulo}: ");
            var s = Console.ReadLine()?.Trim();
            if (s is null) throw new EndOfStreamException();
            if (s.Length > 0) return s;
            Console.WriteLine("  Campo obrigatório.");
        }
    }

    public static int Inteiro(string rotulo, Func<int, bool>? valido = null, string? erro = null)
    {
        while (true)
        {
            var s = Texto(rotulo);
            if (int.TryParse(s, NumberStyles.Integer, PtBr, out var n) && (valido?.Invoke(n) ?? true))
                return n;
            Console.WriteLine($"  {erro ?? "Número inteiro inválido."}");
        }
    }

    public static decimal Decimal(string rotulo)
    {
        while (true)
        {
            var s = Texto(rotulo).Replace("R$", "").Trim();
            // Com vírgula → formato brasileiro (1.234,56). Sem vírgula → ponto é decimal (1234.56).
            // Evita o clássico "1200.50" virar 120050 quando parseado como pt-BR.
            var cultura = s.Contains(',') ? PtBr : CultureInfo.InvariantCulture;
            if (decimal.TryParse(s, NumberStyles.Number, cultura, out var v) && v > 0)
                return v;
            Console.WriteLine("  Valor inválido. Use 1234,56 ou 1234.56.");
        }
    }

    public static DateOnly Data(string rotulo)
    {
        string[] formatos = ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"];
        while (true)
        {
            var s = Texto($"{rotulo} (dd/mm/aaaa)");
            if (DateOnly.TryParseExact(s, formatos, PtBr, DateTimeStyles.None, out var d))
                return d;
            Console.WriteLine("  Data inválida.");
        }
    }
}
