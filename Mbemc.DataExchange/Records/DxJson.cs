// SPDX-License-Identifier: MIT
using System.Collections;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;

namespace Mbemc.DataExchange;

/// <summary>Provides handling of json data.</summary>
public record DxJson
{
    JsonSerializerOptions? options;

    JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions()
        {
            Encoder = Encoder,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyFields = IgnoreReadOnlyFields,
            IgnoreReadOnlyProperties = IgnoreReadOnlyProperties,
            WriteIndented = WriteIndented,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new DxJsonStringRecordConverterFactory());
        options.Converters.Add(new DxJsonIListConverterFactory());
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers = { TypeInfoModifer }
        };
        return options;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether read-only fields are ignored during serialization. A field is read-only if it is marked with the readonly
    /// keyword. The default value is true.
    /// </summary>
    public bool IgnoreReadOnlyFields { get; init; } = true;

    /// <summary>
    /// Determines whether read-only properties are ignored during serialization. A property is read-only if it contains a public getter but not a public
    /// setter. The default value is true.
    /// </summary>
    public bool IgnoreReadOnlyProperties { get; init; } = true;

    /// <summary>
    /// Defines whether JSON should pretty print which includes: indenting nested JSON tokens, adding new lines, and adding white space between property names
    /// and values. The default value is true.
    /// </summary>
    public bool WriteIndented { get; init; } = true;

    /// <summary>The encoder to use when escaping strings.</summary>
    public JavaScriptEncoder Encoder { get; init; } = JavaScriptEncoder.Create(UnicodeRanges.All);

    static bool ShouldSerializeArray(object _, object? value) => value is not null && (value is Array array ? array.Length > 0 : throw new InvalidOperationException($"Value {value} ({value.GetType()}) is not an array!"));

    static bool ShouldSerializeEnumerable(object _, object? value) => value is not null && (value is IEnumerable enumerable ? enumerable.GetEnumerator().MoveNext() : throw new InvalidOperationException($"Value {value} ({value.GetType()}) is not enumerable!"));

    /// <summary>Arrays and lists are written last and not written if empty.</summary>
    /// <param name="typeInfo"></param>
    static void TypeInfoModifer(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (property.PropertyType.IsArray)
            {
                Trace.WriteLine($"Modify ShouldSerializeArray {property.PropertyType.Name} {typeInfo.Type.Name}.{property.Name}");
                property.ShouldSerialize = ShouldSerializeArray;
                if (property.Order == 0) property.Order += 100;
            }
            else if (property.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                Trace.WriteLine($"Modify ShouldSerializeEnumerable {property.PropertyType.Name} {typeInfo.Type.Name}.{property.Name}");
                property.ShouldSerialize = ShouldSerializeEnumerable;
                if (property.Order == 0) property.Order += 100;
            }
        }
    }

    /// <summary>Provides the options used at the json serializer.</summary>
    public JsonSerializerOptions Options => options ??= CreateOptions();

    /// <summary>Loads and deserializes data from the file with the specified <paramref name="stream"/>.</summary>
    /// <typeparam name="TRecord">Type to deserialize.</typeparam>
    /// <param name="stream">Stream to read from.</param>
    /// <returns>Returns a new object of type <typeparamref name="TRecord"/>.</returns>
    public TRecord Load<TRecord>(Stream stream) => JsonSerializer.Deserialize<TRecord>(stream, Options) ?? throw new InvalidDataException($"Could not deserialize {typeof(TRecord).Name}!");

    /// <summary>Clear options instance (frees static memory).</summary>
    public void Reset() => options = null;

    /// <summary>Saves <paramref name="container"/> to the specified <paramref name="stream"/>.</summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="stream"></param>
    /// <param name="container"></param>
    public void Save<TRecord>(Stream stream, TRecord container) => JsonSerializer.Serialize(stream, container, Options);
}
