using DesafioDev.Core.Estoque;
using Microsoft.EntityFrameworkCore;

namespace DesafioDev.Infra;

public sealed class EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(b =>
        {
            // CHECK no banco é a última linha de defesa: mesmo um UPDATE manual não negativa o saldo.
            b.ToTable("Produtos", t => t.HasCheckConstraint("CK_Produtos_Quantidade", "\"Quantidade\" >= 0"));
            b.HasKey(p => p.Codigo);

            // Codigo e Descricao são get-only: propriedade sem setter não é mapeada por convenção,
            // então o mapeamento explícito faz o EF usar o backing field.
            b.Property(p => p.Codigo).ValueGeneratedNever();
            b.Property(p => p.Descricao).HasMaxLength(100).IsRequired();

            // Token de concorrência otimista: o UPDATE sai como
            //   UPDATE Produtos SET Quantidade = @novo WHERE Codigo = @c AND Quantidade = @lido
            // Se outra requisição gravou no meio, afeta 0 linhas e o EF lança DbUpdateConcurrencyException.
            b.Property(p => p.Quantidade).IsConcurrencyToken();
        });

        modelBuilder.Entity<Movimentacao>(b =>
        {
            b.ToTable("Movimentacoes");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).ValueGeneratedOnAdd(); // INTEGER PRIMARY KEY AUTOINCREMENT: id nunca é reaproveitado
            b.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(10);
            b.Property(m => m.Descricao).HasMaxLength(Movimentacao.TamanhoMaximoDescricao).IsRequired();

            b.HasOne<Produto>()
             .WithMany()
             .HasForeignKey(m => m.CodigoProduto)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(m => m.CodigoProduto);
        });
    }
}
