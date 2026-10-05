namespace DesafioDev.Core.Estoque;

public sealed class Produto
{
    public int Codigo { get; }
    public string Descricao { get; }
    public int Quantidade { get; private set; }

    // Os nomes dos parâmetros batem com as propriedades: o EF Core materializa por este construtor.
    public Produto(int codigo, string descricao, int quantidade)
    {
        if (codigo <= 0) throw new ArgumentOutOfRangeException(nameof(codigo));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Descrição obrigatória.", nameof(descricao));
        if (quantidade < 0) throw new ArgumentOutOfRangeException(nameof(quantidade), "Estoque inicial não pode ser negativo.");

        Codigo = codigo;
        Descricao = descricao.Trim();
        Quantidade = quantidade;
    }

    /// <summary>
    /// Única porta de alteração de saldo. Valida, aplica e devolve a movimentação ainda sem Id —
    /// quem persiste (lista em memória ou banco) é quem atribui o identificador.
    /// </summary>
    internal Movimentacao Movimentar(TipoMovimentacao tipo, int quantidade, string descricao, DateTimeOffset dataHora)
    {
        Movimentacao.ValidarArgumentos(tipo, quantidade, descricao);

        var anterior = Quantidade;
        var delta = tipo == TipoMovimentacao.Entrada ? quantidade : -quantidade;

        if (anterior + (long)delta < 0)
            throw new EstoqueInsuficienteException(Codigo, anterior, quantidade);

        Quantidade = checked(anterior + delta);

        return new Movimentacao(0, Codigo, tipo, quantidade, descricao.Trim(), dataHora, anterior, Quantidade);
    }
}

/// <summary>DTOs do arquivo de origem: { "estoque": [ { codigoProduto, descricaoProduto, estoque } ] }.</summary>
public sealed record ProdutoArquivo(int CodigoProduto, string DescricaoProduto, int Estoque);
public sealed record ArquivoEstoque(IReadOnlyList<ProdutoArquivo> Estoque);
