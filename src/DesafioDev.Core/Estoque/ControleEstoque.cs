namespace DesafioDev.Core.Estoque;

/// <summary>Implementação em memória (console e testes). A persistida em SQLite está em DesafioDev.Infra.</summary>
public sealed class ControleEstoque : IServicoEstoque
{
    private readonly Dictionary<int, Produto> _produtos;
    private readonly List<Movimentacao> _historico = new();
    private readonly TimeProvider _relogio;
    private readonly object _lock = new();
    private long _ultimoId;

    public ControleEstoque(IEnumerable<Produto> produtos, TimeProvider? relogio = null, long ultimoIdUtilizado = 0)
    {
        ArgumentNullException.ThrowIfNull(produtos);
        // ToDictionary estoura em código duplicado — melhor quebrar na carga do que ter dois saldos pro mesmo produto.
        _produtos = produtos.ToDictionary(p => p.Codigo);
        _relogio = relogio ?? TimeProvider.System;
        _ultimoId = ultimoIdUtilizado;
    }

    public static ControleEstoque FromArquivo(ArquivoEstoque arquivo, TimeProvider? relogio = null) =>
        new(arquivo.Estoque.Select(p => new Produto(p.CodigoProduto, p.DescricaoProduto, p.Estoque)), relogio);

    public IReadOnlyList<Produto> Produtos
    {
        get { lock (_lock) return _produtos.Values.OrderBy(p => p.Codigo).ToList(); }
    }

    public IReadOnlyList<Movimentacao> Historico
    {
        get { lock (_lock) return _historico.ToList(); }
    }

    public int ConsultarSaldo(int codigoProduto)
    {
        lock (_lock) return ObterProduto(codigoProduto).Quantidade;
    }

    /// <summary>Lança a movimentação e devolve o registro, que já carrega o saldo final do produto.</summary>
    public Movimentacao Movimentar(int codigoProduto, TipoMovimentacao tipo, int quantidade, string descricao)
    {
        Movimentacao.ValidarArgumentos(tipo, quantidade, descricao);

        // O lock cobre validação de saldo + ajuste + geração do id. Sem isso, duas saídas
        // concorrentes passariam juntas pela checagem e deixariam o estoque negativo.
        lock (_lock)
        {
            var mov = ObterProduto(codigoProduto).Movimentar(tipo, quantidade, descricao, _relogio.GetLocalNow());
            // O id só é consumido depois que a regra aceitou a movimentação.
            mov = mov with { Id = ++_ultimoId };
            _historico.Add(mov);
            return mov;
        }
    }

    private Produto ObterProduto(int codigo) =>
        _produtos.TryGetValue(codigo, out var p) ? p : throw new ProdutoNaoEncontradoException(codigo);

    // ---- IServicoEstoque ----

    Task<IReadOnlyList<Produto>> IServicoEstoque.ListarProdutosAsync(CancellationToken ct) =>
        Task.FromResult(Produtos);

    Task<IReadOnlyList<Movimentacao>> IServicoEstoque.ListarMovimentacoesAsync(int? codigoProduto, CancellationToken ct)
    {
        lock (_lock)
            return Task.FromResult<IReadOnlyList<Movimentacao>>(_historico
                .Where(m => codigoProduto is null || m.CodigoProduto == codigoProduto)
                .OrderByDescending(m => m.Id)
                .ToList());
    }

    Task<Movimentacao?> IServicoEstoque.ObterMovimentacaoAsync(long id, CancellationToken ct)
    {
        lock (_lock) return Task.FromResult(_historico.FirstOrDefault(m => m.Id == id));
    }

    Task<Movimentacao> IServicoEstoque.MovimentarAsync(int codigoProduto, TipoMovimentacao tipo, int quantidade, string descricao, CancellationToken ct) =>
        Task.FromResult(Movimentar(codigoProduto, tipo, quantidade, descricao));
}
