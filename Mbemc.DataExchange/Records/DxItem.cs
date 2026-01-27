// SPDX-License-Identifier: MIT
using System.Diagnostics;

namespace Mbemc.DataExchange;

/// <summary>Provides a xchange item.</summary>
[DebuggerDisplay("XChangeItem Name = {Name}, Unit = {Unit}")]
public record DxItem : IDxName, IDxDescription, IDxUnit
{
    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string? Unit { get; init; }
}
