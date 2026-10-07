using System.Text.Json;

namespace JpHelpDesk.Api.Models.Enums;

public static class EnumExtensions
{
    /// <summary>
    /// Representação do enum usada na API e no banco (ex.: EmAtendimento -> "EM_ATENDIMENTO").
    /// </summary>
    public static string ToApiString<TEnum>(this TEnum valor) where TEnum : struct, Enum =>
        JsonNamingPolicy.SnakeCaseUpper.ConvertName(valor.ToString());
}
