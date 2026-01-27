// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides an xchange data container.</summary>
public record DxFile : DxDocument
{
    /// <summary>Gets the file format descriptor</summary>
    public string Type { get; init; } = "EMC Data Exchange Format";

    /// <summary>Gets the file version used.</summary>
    public Version Version { get; init; } = new(2, 0);
}
