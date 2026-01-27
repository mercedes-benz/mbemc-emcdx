// SPDX-License-Identifier: MIT
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mbemc.DataExchange;

class DxJsonStringRecordConverter<TRecord> : JsonConverter<TRecord>
    where TRecord : IDxStoreAsString<TRecord>, new()
{
    #region Private Fields

    readonly TRecord instance = new();

    #endregion Private Fields

    #region Public Methods

    public override bool CanConvert(Type typeToConvert) => typeToConvert == instance.GetType();

    public override TRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? stringValue;
        switch (reader.TokenType)
        {
            case JsonTokenType.String: stringValue = reader.GetString(); break;
            case JsonTokenType.Number: stringValue = reader.GetDouble().ToString("R", CultureInfo.InvariantCulture); break;
            case JsonTokenType.Null: reader.Skip(); return default;
            default: throw new JsonException($"Expecting TokenType = JsonTokenType.String instead of JsonTokenType.{reader.TokenType} when reading {typeToConvert} value!");
        }
        return stringValue is null ? default : instance.Parse(stringValue, CultureInfo.InvariantCulture);
    }

    public override TRecord ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);
        return instance.Parse(reader.GetString(), CultureInfo.InvariantCulture) ?? new();
    }

    public override void Write(Utf8JsonWriter writer, TRecord value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("R", CultureInfo.InvariantCulture));

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TRecord value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString("R", CultureInfo.InvariantCulture));

    #endregion Public Methods
}
