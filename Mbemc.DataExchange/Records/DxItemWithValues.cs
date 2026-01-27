// SPDX-License-Identifier: MIT
using System.Diagnostics;

namespace Mbemc.DataExchange;

/// <summary>Provides an exchange item with binary, generator or simple values.</summary>
[DebuggerDisplay("DxItem Name = {Name}, Unit = {Unit}, Values = {ValueCount}")]
public record DxItemWithValues : DxItem
{
    /// <summary>Gets or sets a binary data source to be used for the values of the variable. This is exclusive with <see cref="BinaryValues"/>.</summary>
    public DxBinaryValues? BinaryValues { get; init; }

    /// <summary>Gets or sets an array with values used for the variable. This is exclusive with <see cref="Values"/>.</summary>
    public IList<double>? Values { get; init; }

    /// <summary>Gets the number of values present at the variable.</summary>
    /// <remarks>This is taken from <see cref="Values"/> or <see cref="BinaryValues"/>. Only one of the above instances shall be used at a time!</remarks>
    public virtual long ValueCount => Values?.Count ?? BinaryValues?.ValueCount ?? 0;
}
