using System.Text.Json;

namespace DesafioDev.Infra;

public static class ArquivoJson
{
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static T Carregar<T>(string caminho)
    {
        using var stream = File.OpenRead(caminho);
        return JsonSerializer.Deserialize<T>(stream, Opcoes)
               ?? throw new InvalidDataException($"Arquivo {caminho} vazio ou inválido.");
    }
}
