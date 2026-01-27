// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mbemc.DataExchange;

sealed class DxJsonIListConverterFactory : JsonConverterFactory
{
    #region Public Methods

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsInterface && typeToConvert.IsGenericType && typeToConvert.GenericTypeArguments.Length == 1 && typeToConvert.GetGenericTypeDefinition() == typeof(IList<>);

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var iface = typeToConvert;
        var recordType = iface.GetGenericArguments().Single();
        var converterType = typeof(DxJsonIListConverter<>).MakeGenericType([recordType]);
        var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
        return converter;
    }

    #endregion Public Methods
}
