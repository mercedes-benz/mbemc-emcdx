// SPDX-License-Identifier: MIT
using System.Diagnostics;

namespace Mbemc.DataExchange;

/// <summary>Provides a xchange item.</summary>
[DebuggerDisplay("XChangeDataItem Name = {Name}")]
public record DxDataItem : IEquatable<DxDataItem>, IDxName, IDxDescription
{
    /// <inheritdoc/>
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <summary>Gets the list of variables at the data item.</summary>
    public IList<DxVariable> Variables { get; init; } = [];

    /// <summary>Gets the list of mappings at the data item.</summary>
    public IList<DxMapping> Mappings { get; init; } = [];
}
