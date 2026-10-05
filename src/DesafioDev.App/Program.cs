using System.Globalization;
using System.Text;
using DesafioDev.App;
using DesafioDev.Core.Comissao;
using DesafioDev.Core.Estoque;
using DesafioDev.Core.Juros;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

// O estoque vive enquanto o processo vive: os ids e o histórico de movimentação
// precisam sobreviver entre uma ida e outra ao menu.
var estoque = ControleEstoque.FromArquivo(JsonDados.Carregar<ArquivoEstoque>("estoque.json"));
var calculadoraJuros = new CalculadoraJuros();

try
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("==== DESAFIO DEV ====");
        Console.WriteLine("1 - Comissão dos vendedores");
        Console.WriteLine("2 - Movimentar estoque");
        Console.WriteLine("3 - Calcular juros por atraso");
        Console.WriteLine("0 - Sair");

        var opcao = Entrada.Inteiro("Opção", n => n is >= 0 and <= 3, "Escolha 0, 1, 2 ou 3.");
        Console.WriteLine();

        switch (opcao)
        {
            case 1: Telas.Comissao(); break;
            case 2: Telas.Estoque(estoque); break;
            case 3: Telas.Juros(calculadoraJuros); break;
            case 0: return;
        }
    }
}
catch (EndOfStreamException)
{
    // stdin encerrado (Ctrl+Z / Ctrl+D ou entrada redirecionada) — sai sem stack trace.
}

internal static class Telas
{
    public static void Comissao()
    {
        var arquivo = JsonDados.Carregar<ArquivoVendas>("vendas.json");
        var resultado = CalculadoraComissao.Calcular(arquivo.Vendas);

        Console.WriteLine($"{"Vendedor",-18}{"Vendas",7}{"Comiss.",9}{"Total vendido",16}{"Comissão",13}");
        Console.WriteLine(new string('-', 63));
        foreach (var c in resultado)
            Console.WriteLine($"{c.Vendedor,-18}{c.QuantidadeVendas,7}{c.VendasComissionadas,9}{c.TotalVendido,16:C}{c.TotalComissao,13:C}");
        Console.WriteLine(new string('-', 63));
        Console.WriteLine($"{"TOTAL",-18}{resultado.Sum(c => c.QuantidadeVendas),7}{resultado.Sum(c => c.VendasComissionadas),9}" +
                          $"{resultado.Sum(c => c.TotalVendido),16:C}{resultado.Sum(c => c.TotalComissao),13:C}");
    }

    public static void Estoque(ControleEstoque estoque)
    {
        Console.WriteLine($"{"Código",-8}{"Produto",-30}{"Saldo",8}");
        foreach (var p in estoque.Produtos)
            Console.WriteLine($"{p.Codigo,-8}{p.Descricao,-30}{p.Quantidade,8}");
        Console.WriteLine();

        // Valida o produto antes de pedir o resto — não faz sentido digitar tudo para descobrir no fim que o código não existe.
        var codigo = Entrada.Inteiro("Código do produto",
            n => estoque.Produtos.Any(p => p.Codigo == n), "Produto não encontrado.");
        var tipo = (TipoMovimentacao)Entrada.Inteiro("Tipo (1 - Entrada, 2 - Saída)", n => n is 1 or 2, "Informe 1 ou 2.");
        var quantidade = Entrada.Inteiro("Quantidade", n => n > 0, "Quantidade deve ser maior que zero.");
        var descricao = Entrada.Texto("Descrição da movimentação");

        try
        {
            var mov = estoque.Movimentar(codigo, tipo, quantidade, descricao);
            Console.WriteLine();
            Console.WriteLine($"Movimentação #{mov.Id} registrada em {mov.DataHora:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"  {(mov.Tipo == TipoMovimentacao.Entrada ? "Entrada" : "Saída")} de {mov.Quantidade} un. — \"{mov.Descricao}\"");
            Console.WriteLine($"  Estoque do produto {mov.CodigoProduto}: {mov.EstoqueAnterior} → {mov.EstoqueFinal}");
        }
        catch (EstoqueInsuficienteException ex)
        {
            Console.WriteLine($"Movimentação recusada: {ex.Message}");
        }
    }

    public static void Juros(CalculadoraJuros calculadora)
    {
        var valor = Entrada.Decimal("Valor (R$)");
        var vencimento = Entrada.Data("Vencimento");
        var r = calculadora.Calcular(valor, vencimento);

        Console.WriteLine();
        Console.WriteLine($"Data do cálculo : {r.DataCalculo:dd/MM/yyyy}");
        Console.WriteLine($"Dias em atraso  : {r.DiasAtraso}");
        Console.WriteLine($"Taxa            : {r.TaxaDiaria:P1} ao dia");
        Console.WriteLine($"Valor original  : {r.ValorOriginal:C}");
        Console.WriteLine($"Juros           : {r.Juros:C}");
        Console.WriteLine($"Valor atualizado: {r.ValorAtualizado:C}");
    }
}
