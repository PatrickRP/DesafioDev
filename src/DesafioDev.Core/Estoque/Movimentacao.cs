namespace DesafioDev.Core.Estoque;

public enum TipoMovimentacao
{
    Entrada = 1,
    Saida = 2
}

public sealed record Movimentacao(
    long Id,
    int CodigoProduto,
    TipoMovimentacao Tipo,
    int Quantidade,
    string Descricao,
    DateTimeOffset DataHora,
    int EstoqueAnterior,
    int EstoqueFinal)
{
    public const int TamanhoMaximoDescricao = 200;

    public static void ValidarArgumentos(TipoMovimentacao tipo, int quantidade, string? descricao)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de movimentação inválido.");
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), quantidade, "Quantidade deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da movimentação é obrigatória.", nameof(descricao));
        if (descricao.Trim().Length > TamanhoMaximoDescricao)
            throw new ArgumentException($"Descrição deve ter no máximo {TamanhoMaximoDescricao} caracteres.", nameof(descricao));
    }
}

public sealed class ProdutoNaoEncontradoException(int codigo)
    : InvalidOperationException($"Produto {codigo} não encontrado.")
{
    public int CodigoProduto { get; } = codigo;
}

public sealed class EstoqueInsuficienteException(int codigo, int disponivel, int solicitado)
    : InvalidOperationException($"Estoque insuficiente para o produto {codigo}: disponível {disponivel}, solicitado {solicitado}.")
{
    public int CodigoProduto { get; } = codigo;
    public int Disponivel { get; } = disponivel;
    public int Solicitado { get; } = solicitado;
}

/// <summary>O saldo mudou entre a leitura e a gravação em todas as tentativas — o cliente deve repetir.</summary>
public sealed class ConflitoConcorrenciaException(int codigo)
    : InvalidOperationException($"O estoque do produto {codigo} foi alterado por outra operação. Tente novamente.")
{
    public int CodigoProduto { get; } = codigo;
}
