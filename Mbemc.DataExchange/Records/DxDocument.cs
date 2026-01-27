// SPDX-License-Identifier: MIT
using System.Text.Json;

namespace Mbemc.DataExchange;

/// <summary>Provides an xchange data container.</summary>
public record DxDocument : IDxName, IDxDescription
{
    /// <inheritdoc/>
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <summary>Gets the data items.</summary>
    public IList<DxDataItem> Data { get; init; } = Array.Empty<DxDataItem>();

    /// <summary>Gets the metadata associated with this file.</summary>
    public JsonDocument? Meta { get; init; }

    /// <summary>Creates a new <see cref="DxFile"/> instance with the <see cref="DxDocument"/> content.</summary>
    /// <returns>Returns a new <see cref="DxFile"/> instance.</returns>
    public DxFile ToFile() => new() { Data = Data, Name = Name, Description = Description, Meta = Meta };
}
