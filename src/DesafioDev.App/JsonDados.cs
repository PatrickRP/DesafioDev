using System.Text.Json;

namespace DesafioDev.App;

internal static class JsonDados
{
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static T Carregar<T>(string arquivo)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Data", arquivo);
        using var stream = File.OpenRead(caminho);
        return JsonSerializer.Deserialize<T>(stream, Opcoes)
               ?? throw new InvalidDataException($"Arquivo {arquivo} vazio ou inválido.");
    }
}
