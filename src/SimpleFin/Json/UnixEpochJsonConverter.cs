// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleFin.Json;

/// <summary>
/// Converts SimpleFIN timestamps, transmitted as Unix epoch seconds, to and from
/// <see cref="DateTimeOffset"/>.
/// </summary>
public sealed class UnixEpochJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        long seconds = reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt64(),
            JsonTokenType.String => long.Parse(reader.GetString()!),
            _ => throw new JsonException("Expected a Unix epoch timestamp."),
        };

        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options
    )
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
