using DesafioDev.Core.Estoque;
using Microsoft.EntityFrameworkCore;

namespace DesafioDev.Infra;

public sealed class ServicoEstoqueEf(EstoqueDbContext db, TimeProvider relogio) : IServicoEstoque
{
    private const int MaximoTentativas = 3;

    public async Task<IReadOnlyList<Produto>> ListarProdutosAsync(CancellationToken ct = default) =>
        await db.Produtos.AsNoTracking().OrderBy(p => p.Codigo).ToListAsync(ct);

    public async Task<IReadOnlyList<Movimentacao>> ListarMovimentacoesAsync(int? codigoProduto = null, CancellationToken ct = default)
    {
        var query = db.Movimentacoes.AsNoTracking();
        if (codigoProduto is int codigo)
            query = query.Where(m => m.CodigoProduto == codigo);

        // Ordena por Id e não por DataHora: o provider SQLite não traduz ORDER BY de DateTimeOffset,
        // e o Id AUTOINCREMENT já reflete a ordem de gravação.
        return await query.OrderByDescending(m => m.Id).ToListAsync(ct);
    }

    public Task<Movimentacao?> ObterMovimentacaoAsync(long id, CancellationToken ct = default) =>
        db.Movimentacoes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Movimentacao> MovimentarAsync(
        int codigoProduto, TipoMovimentacao tipo, int quantidade, string descricao, CancellationToken ct = default)
    {
        Movimentacao.ValidarArgumentos(tipo, quantidade, descricao);

        for (var tentativa = 1; ; tentativa++)
        {
            // Cada tentativa relê o saldo do banco; sem o Clear o EF devolveria a instância já rastreada (e velha).
            db.ChangeTracker.Clear();

            var produto = await db.Produtos.SingleOrDefaultAsync(p => p.Codigo == codigoProduto, ct)
                          ?? throw new ProdutoNaoEncontradoException(codigoProduto);

            // A regra (saldo suficiente, ajuste) continua no domínio; o banco só garante que ninguém gravou no meio.
            var mov = produto.Movimentar(tipo, quantidade, descricao, relogio.GetLocalNow());
            db.Movimentacoes.Add(mov);

            try
            {
                // UPDATE do produto + INSERT da movimentação na mesma transação implícita do SaveChanges.
                await db.SaveChangesAsync(ct);
                return mov; // o EF preenche o Id gerado pelo banco
            }
            catch (DbUpdateConcurrencyException) when (tentativa < MaximoTentativas)
            {
                // Outra requisição alterou o saldo entre a leitura e a gravação: relê e reaplica a regra.
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflitoConcorrenciaException(codigoProduto);
            }
        }
    }
}
