using JpHelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JpHelpDesk.Api.Data.Converters;

/// <summary>
/// Grava enums como texto em UPPER_SNAKE_CASE (ex.: EmAtendimento -> "EM_ATENDIMENTO")
/// e converte de volta ao ler do banco.
/// </summary>
public class UpperSnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => value.ToApiString(),
    text => EnumExtensions.FromApiString<TEnum>(text))
    where TEnum : struct, Enum;
