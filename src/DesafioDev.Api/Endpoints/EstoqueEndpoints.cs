using DesafioDev.Core.Estoque;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DesafioDev.Api.Endpoints;

public sealed record ProdutoResponse(int Codigo, string Descricao, int Quantidade);

// Tipo e Descricao anuláveis: distingue "não enviado" de valor inválido e devolve erro por campo.
public sealed record NovaMovimentacaoRequest(int CodigoProduto, TipoMovimentacao? Tipo, int Quantidade, string? Descricao);

public static class EstoqueEndpoints
{
    public static RouteGroupBuilder MapEstoque(this RouteGroupBuilder api)
    {
        var estoque = api.MapGroup("/estoque");

        estoque.MapGet("/produtos", async (IServicoEstoque servico, CancellationToken ct) =>
            TypedResults.Ok((await servico.ListarProdutosAsync(ct))
                .Select(p => new ProdutoResponse(p.Codigo, p.Descricao, p.Quantidade))));

        estoque.MapGet("/movimentacoes", async (int? codigoProduto, IServicoEstoque servico, CancellationToken ct) =>
            TypedResults.Ok(await servico.ListarMovimentacoesAsync(codigoProduto, ct)));

        estoque.MapGet("/movimentacoes/{id:long}", async Task<Results<Ok<Movimentacao>, NotFound>> (
                long id, IServicoEstoque servico, CancellationToken ct) =>
            await servico.ObterMovimentacaoAsync(id, ct) is { } mov ? TypedResults.Ok(mov) : TypedResults.NotFound())
            .WithName("ObterMovimentacao");

        estoque.MapPost("/movimentacoes", async Task<Results<CreatedAtRoute<Movimentacao>, ValidationProblem>> (
            NovaMovimentacaoRequest req, IServicoEstoque servico, CancellationToken ct) =>
        {
            var erros = Validar(req);
            if (erros.Count > 0)
                return TypedResults.ValidationProblem(erros);

            // Saldo insuficiente / produto inexistente viram 422 / 404 no DominioExceptionHandler.
            var mov = await servico.MovimentarAsync(req.CodigoProduto, req.Tipo!.Value, req.Quantidade, req.Descricao!, ct);
            return TypedResults.CreatedAtRoute(mov, "ObterMovimentacao", new { id = mov.Id });
        });

        return api;
    }

    // Chaves em camelCase para casar com os nomes dos controles do formulário no Angular.
    private static Dictionary<string, string[]> Validar(NovaMovimentacaoRequest req)
    {
        var erros = new Dictionary<string, string[]>();

        if (req.CodigoProduto <= 0)
            erros["codigoProduto"] = ["Informe o produto."];
        if (req.Tipo is null || !Enum.IsDefined(req.Tipo.Value))
            erros["tipo"] = ["Informe Entrada ou Saida."];
        if (req.Quantidade <= 0)
            erros["quantidade"] = ["Quantidade deve ser maior que zero."];
        if (string.IsNullOrWhiteSpace(req.Descricao))
            erros["descricao"] = ["Descreva a movimentação."];
        else if (req.Descricao.Trim().Length > Movimentacao.TamanhoMaximoDescricao)
            erros["descricao"] = [$"Máximo de {Movimentacao.TamanhoMaximoDescricao} caracteres."];

        return erros;
    }
}
