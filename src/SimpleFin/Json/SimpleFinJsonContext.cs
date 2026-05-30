// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleFin.Models;

namespace SimpleFin.Json;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for SimpleFIN model types, enabling
/// trim- and AOT-safe JSON serialization.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(AccountSet))]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(Transaction))]
[JsonSerializable(typeof(Connection))]
[JsonSerializable(typeof(SimpleFinError))]
[JsonSerializable(typeof(CustomCurrency))]
[JsonSerializable(typeof(SimpleFinInfo))]
public sealed partial class SimpleFinJsonContext : JsonSerializerContext { }
