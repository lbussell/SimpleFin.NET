// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleFin.Json;

/// <summary>
/// Converts optional SimpleFIN timestamps, transmitted as Unix epoch seconds, to and from
/// <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. A JSON <c>null</c> maps to
/// <see langword="null"/>.
/// </summary>
public sealed class NullableUnixEpochJsonConverter : JsonConverter<DateTimeOffset?>
{
    /// <inheritdoc />
    public override DateTimeOffset? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return SimpleFinJsonReader.ReadUnixEpoch(ref reader);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset? value,
        JsonSerializerOptions options
    )
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
        }
    }
}
