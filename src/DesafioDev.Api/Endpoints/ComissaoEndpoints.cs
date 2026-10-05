using DesafioDev.Core.Comissao;
using Microsoft.AspNetCore.Mvc;

namespace DesafioDev.Api.Endpoints;

public sealed record ComissoesResponse(
    IReadOnlyList<ComissaoVendedor> Vendedores,
    int QuantidadeVendas,
    decimal TotalVendido,
    decimal TotalComissao);

public static class ComissaoEndpoints
{
    public static RouteGroupBuilder MapComissoes(this RouteGroupBuilder api)
    {
        api.MapGet("/comissoes", ([FromServices] ArquivoVendas arquivo) =>
        {
            var vendedores = CalculadoraComissao.Calcular(arquivo.Vendas);
            return TypedResults.Ok(new ComissoesResponse(
                vendedores,
                vendedores.Sum(v => v.QuantidadeVendas),
                vendedores.Sum(v => v.TotalVendido),
                vendedores.Sum(v => v.TotalComissao)));
        });

        return api;
    }
}
