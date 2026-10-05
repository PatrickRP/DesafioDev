using DesafioDev.Core.Estoque;
using Microsoft.AspNetCore.Diagnostics;

namespace DesafioDev.Api;

/// <summary>
/// Traduz exceções de domínio em ProblemDetails (RFC 9457). Os endpoints ficam livres de try/catch
/// e o front recebe sempre o mesmo formato de erro.
/// </summary>
internal sealed class DominioExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            ProdutoNaoEncontradoException => (StatusCodes.Status404NotFound, "Produto não encontrado"),
            EstoqueInsuficienteException => (StatusCodes.Status422UnprocessableEntity, "Estoque insuficiente"),
            ConflitoConcorrenciaException => (StatusCodes.Status409Conflict, "Conflito de concorrência"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            _ => (0, null)
        };

        if (status == 0)
            return false; // deixa o pipeline padrão devolver 500

        ctx.Response.StatusCode = status;

        var contexto = new ProblemDetailsContext
        {
            HttpContext = ctx,
            Exception = ex,
            ProblemDetails = { Status = status, Title = titulo, Detail = ex.Message }
        };

        if (ex is EstoqueInsuficienteException e)
        {
            contexto.ProblemDetails.Extensions["disponivel"] = e.Disponivel;
            contexto.ProblemDetails.Extensions["solicitado"] = e.Solicitado;
        }

        return await problemDetails.TryWriteAsync(contexto);
    }
}
