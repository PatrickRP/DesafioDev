using DesafioDev.Core.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DesafioDev.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddEstoqueSqlite(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EstoqueDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IServicoEstoque, ServicoEstoqueEf>();
        return services;
    }

    /// <summary>Aplica as migrations e, com o banco vazio, carrega os produtos do estoque.json.</summary>
    public static async Task InicializarEstoqueAsync(this IServiceProvider services, ArquivoEstoque carga, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

        await db.Database.MigrateAsync(ct);

        // WAL deixa leituras correrem em paralelo com a escrita (no modo padrão, o writer bloqueia leitores).
        // É persistente no arquivo, então basta rodar uma vez — repetir é inofensivo.
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", ct);

        if (await db.Produtos.AnyAsync(ct))
            return;

        db.Produtos.AddRange(carga.Estoque.Select(p => new Produto(p.CodigoProduto, p.DescricaoProduto, p.Estoque)));
        await db.SaveChangesAsync(ct);
    }
}
