using DesafioDev.Core.Juros;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DesafioDev.Api.Endpoints;

public static class JurosEndpoints
{
    public static RouteGroupBuilder MapJuros(this RouteGroupBuilder api)
    {
        // GET: cálculo puro, sem efeito colateral. Query string usa cultura invariante:
        // valor=1234.56&vencimento=2026-09-25
        api.MapGet("/juros", Results<Ok<ResultadoJuros>, ValidationProblem> (
            decimal valor, DateOnly vencimento, CalculadoraJuros calculadora) =>
        {
            if (valor <= 0)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["valor"] = ["Valor deve ser maior que zero."]
                });

            return TypedResults.Ok(calculadora.Calcular(valor, vencimento));
        });

        return api;
    }
}
