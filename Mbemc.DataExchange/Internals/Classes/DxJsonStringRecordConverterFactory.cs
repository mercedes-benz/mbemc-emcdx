// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mbemc.DataExchange;

sealed class DxJsonStringRecordConverterFactory : JsonConverterFactory
{
    #region Public Methods

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => typeToConvert.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDxStoreAsString<>));

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var iface = typeToConvert.GetInterfaces().Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDxStoreAsString<>));
        var recordType = iface.GetGenericArguments().Single();
        var converterType = typeof(DxJsonStringRecordConverter<>).MakeGenericType([recordType]);
        var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
        return converter;
    }

    #endregion Public Methods
}
