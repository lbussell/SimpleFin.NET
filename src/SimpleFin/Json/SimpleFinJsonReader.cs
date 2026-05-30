// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text.Json;

namespace SimpleFin.Json;

internal static class SimpleFinJsonReader
{
    internal static decimal ReadDecimal(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            try
            {
                return reader.GetDecimal();
            }
            catch (FormatException ex)
            {
                throw new JsonException("Expected a decimal value.", ex);
            }
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a numeric string for a decimal value.");
        }

        string? value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Expected a non-empty numeric string for a decimal value.");
        }

        return ParseDecimal(value);
    }

    internal static decimal? ReadNullableDecimal(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String && string.IsNullOrEmpty(reader.GetString()))
        {
            return null;
        }

        return ReadDecimal(ref reader);
    }

    internal static DateTimeOffset ReadUnixEpoch(ref Utf8JsonReader reader)
    {
        long seconds = reader.TokenType switch
        {
            JsonTokenType.Number => ReadInt64(ref reader),
            JsonTokenType.String => ParseInt64(reader.GetString()),
            _ => throw new JsonException("Expected a Unix epoch timestamp."),
        };

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new JsonException("Expected a Unix epoch timestamp in range.", ex);
        }
    }

    private static decimal ParseDecimal(string value)
    {
        if (
            !decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal result
            )
        )
        {
            throw new JsonException("Expected a numeric string for a decimal value.");
        }

        return result;
    }

    private static long ReadInt64(ref Utf8JsonReader reader)
    {
        try
        {
            return reader.GetInt64();
        }
        catch (FormatException ex)
        {
            throw new JsonException("Expected a Unix epoch timestamp.", ex);
        }
    }

    private static long ParseInt64(string? value)
    {
        if (
            !long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long result
            )
        )
        {
            throw new JsonException("Expected a Unix epoch timestamp.");
        }

        return result;
    }
}
