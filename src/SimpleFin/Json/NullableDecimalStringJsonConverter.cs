// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleFin.Json;

/// <summary>
/// Converts optional SimpleFIN monetary values, transmitted as JSON numeric strings, to and
/// from <see cref="Nullable{T}"/> of <see cref="decimal"/>. A JSON <c>null</c> maps to
/// <see langword="null"/>.
/// </summary>
public sealed class NullableDecimalStringJsonConverter : JsonConverter<decimal?>
{
    /// <inheritdoc />
    public override decimal? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return SimpleFinJsonReader.ReadNullableDecimal(ref reader);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
