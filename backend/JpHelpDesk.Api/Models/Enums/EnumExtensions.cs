using System.Text.Json;

namespace JpHelpDesk.Api.Models.Enums;

public static class EnumExtensions
{
    /// <summary>
    /// Representação do enum usada na API e no banco (ex.: EmAtendimento -> "EM_ATENDIMENTO").
    /// </summary>
    public static string ToApiString<TEnum>(this TEnum valor) where TEnum : struct, Enum =>
        JsonNamingPolicy.SnakeCaseUpper.ConvertName(valor.ToString());

    /// <summary>
    /// Converte o texto da API/banco de volta para o enum (ex.: "EM_ATENDIMENTO" -> EmAtendimento).
    /// </summary>
    public static TEnum FromApiString<TEnum>(string texto) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(texto.Replace("_", string.Empty), ignoreCase: true);
}
