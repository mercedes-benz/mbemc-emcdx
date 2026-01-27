// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

#pragma warning disable CS0618

/// <summary>Interface for records saved to storage as string.</summary>
[Obsolete($"Do not use directly without understanding the implications! Use the typed variant {nameof(IDxStoreAsString)}<TResult> instead!")]
public interface IDxStoreAsString : IFormattable
{
}

/// <summary>Interface for records saved to storage as string.</summary>
/// <remarks>
/// <para>Serializers use <see cref="IFormattable.ToString(string, IFormatProvider)"/> with format: "*" and formatProvider: null.</para>
/// <para>Deserializers use <see cref="Parse(string, IFormatProvider)"/> with formatProvider: null.</para>
/// </remarks>
/// <typeparam name="TRecord"></typeparam>
public interface IDxStoreAsString<TRecord> : IDxStoreAsString where TRecord : IDxStoreAsString<TRecord>, new()
{
    #region Public Methods

    /// <summary>Parses a storage string back to a new <typeparamref name="TRecord"/> instance.</summary>
    /// <param name="storageString">String representation of the record.</param>
    /// <param name="formatProvider">Format provider used to generate the string.</param>
    /// <returns>Returns a new <typeparamref name="TRecord"/> instance.</returns>
    TRecord? Parse(string? storageString, IFormatProvider? formatProvider);

    #endregion Public Methods
}
