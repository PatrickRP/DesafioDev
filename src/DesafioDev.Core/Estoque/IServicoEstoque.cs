namespace DesafioDev.Core.Estoque;

public interface IServicoEstoque
{
    Task<IReadOnlyList<Produto>> ListarProdutosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Movimentacao>> ListarMovimentacoesAsync(int? codigoProduto = null, CancellationToken ct = default);
    Task<Movimentacao?> ObterMovimentacaoAsync(long id, CancellationToken ct = default);
    Task<Movimentacao> MovimentarAsync(int codigoProduto, TipoMovimentacao tipo, int quantidade, string descricao, CancellationToken ct = default);
}
