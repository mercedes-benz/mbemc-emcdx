// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mbemc.DataExchange;

class DxJsonIListConverter<TRecord> : JsonConverter<IList<TRecord>>
{
    #region Private Fields

    static readonly IList<TRecord> Empty = Array.Empty<TRecord>();

    #endregion Private Fields

    #region Public Methods

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(IList<TRecord>);

    public override IList<TRecord>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            case JsonTokenType.String:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.StartObject:
            {
                if (!DxExtensions.EnableMatlabArrayExtensions) break;
                //handle matlab bug: when writing an array with only a single element, matlab only writes the object instead of an array with the single object in it.
                var single = JsonSerializer.Deserialize<TRecord>(ref reader, options);
                return single is null ? Empty : [single];
            }
            case JsonTokenType.StartArray:
            {
                try
                {
                    var array = JsonSerializer.Deserialize<TRecord[]>(ref reader, options);
                    return array is null ? Empty : array;
                }
                catch (JsonException)
                {
                    if (DxExtensions.EnableMatlabArrayExtensions)
                    {
                        //matlab may save all values as double, even integer types. since json allows this and system.text.json does not, we need to convert
                        //see bug https://github.com/dotnet/runtime/issues/40596
                        var recordType = typeof(TRecord);
                        //do this only for native value types
                        if (recordType.IsValueType && recordType.UnderlyingSystemType == recordType && recordType != typeof(double))
                        {
                            var array = JsonSerializer.Deserialize<double[]>(ref reader, options);
                            return array is null ? Empty : [.. array.Select(v => (TRecord)Convert.ChangeType(v, recordType))];
                        }
                    }
                    throw;
                }
            }
            case JsonTokenType.Null: reader.Skip(); return null;
        }
        throw new JsonException($"Expecting TokenType = JsonTokenType.Array instead of JsonTokenType.{reader.TokenType} when reading {typeToConvert} value!");
    }

    public override void Write(Utf8JsonWriter writer, IList<TRecord> value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value.ToArray(), options);

    #endregion Public Methods
}
