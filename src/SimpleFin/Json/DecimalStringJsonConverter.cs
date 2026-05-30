// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleFin.Json;

/// <summary>
/// Converts SimpleFIN monetary values, which are transmitted as JSON numeric strings
/// (for example <c>"-33293.43"</c>), to and from <see cref="decimal"/>.
/// </summary>
public sealed class DecimalStringJsonConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return SimpleFinJsonReader.ReadDecimal(ref reader);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
