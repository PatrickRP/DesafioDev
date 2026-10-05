using DesafioDev.Core.Estoque;

namespace DesafioDev.Tests;

public class EstoqueTests
{
    private static ControleEstoque Criar() => new(
    [
        new Produto(101, "Caneta Azul", 150),
        new Produto(102, "Caderno Universitário", 75)
    ], new RelogioFixo(2026, 10, 5));

    [Fact]
    public void Entrada_soma_e_retorna_saldo_final()
    {
        var mov = Criar().Movimentar(101, TipoMovimentacao.Entrada, 50, "Compra NF 987");

        Assert.Equal(150, mov.EstoqueAnterior);
        Assert.Equal(200, mov.EstoqueFinal);
    }

    [Fact]
    public void Saida_subtrai_e_retorna_saldo_final()
    {
        var estoque = Criar();
        var mov = estoque.Movimentar(102, TipoMovimentacao.Saida, 75, "Venda");

        Assert.Equal(0, mov.EstoqueFinal);
        Assert.Equal(0, estoque.ConsultarSaldo(102));
    }

    [Fact]
    public void Saida_maior_que_saldo_e_recusada_sem_efeito_colateral()
    {
        var estoque = Criar();

        var ex = Assert.Throws<EstoqueInsuficienteException>(
            () => estoque.Movimentar(102, TipoMovimentacao.Saida, 76, "Venda"));

        Assert.Equal(75, ex.Disponivel);
        Assert.Equal(75, estoque.ConsultarSaldo(102));
        Assert.Empty(estoque.Historico);

        // Tentativa recusada não consome id.
        Assert.Equal(1, estoque.Movimentar(101, TipoMovimentacao.Entrada, 1, "Ajuste").Id);
    }

    [Fact]
    public void Ids_sao_sequenciais_e_unicos()
    {
        var estoque = Criar();
        var ids = Enumerable.Range(0, 5)
            .Select(_ => estoque.Movimentar(101, TipoMovimentacao.Entrada, 1, "Entrada").Id)
            .ToArray();

        Assert.Equal(new long[] { 1, 2, 3, 4, 5 }, ids);
    }

    [Fact]
    public void Movimentacoes_concorrentes_nao_duplicam_id_nem_negativam_saldo()
    {
        var estoque = Criar(); // 150 canetas
        var ids = new System.Collections.Concurrent.ConcurrentBag<long>();

        Parallel.For(0, 300, _ =>
        {
            try { ids.Add(estoque.Movimentar(101, TipoMovimentacao.Saida, 1, "Saída").Id); }
            catch (EstoqueInsuficienteException) { }
        });

        Assert.Equal(150, ids.Count);
        Assert.Equal(150, ids.Distinct().Count());
        Assert.Equal(0, estoque.ConsultarSaldo(101));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Quantidade_nao_positiva_e_invalida(int qtd) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Criar().Movimentar(101, TipoMovimentacao.Entrada, qtd, "X"));

    [Fact]
    public void Descricao_e_obrigatoria() =>
        Assert.Throws<ArgumentException>(() => Criar().Movimentar(101, TipoMovimentacao.Entrada, 1, "   "));

    [Fact]
    public void Produto_inexistente() =>
        Assert.Throws<ProdutoNaoEncontradoException>(() => Criar().Movimentar(999, TipoMovimentacao.Entrada, 1, "X"));
}
