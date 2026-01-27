// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>
/// Provides the available placement types used in the JSON file. This is not written to the JSON as a property; instead, it is read implicitly by checking the
/// "Values", "Source" and "Content" properties.
/// </summary>
public enum DxDataPlacement
{
    /// <summary>Autoselect <see cref="Array"/> or <see cref="BinaryFile"/> by value count limit.</summary>
    Autoselect = 0,

    /// <summary>Embedded values (float64 array) at "Values" property.</summary>
    Array,

    /// <summary>Embedded data or values at "Content" property.</summary>
    Content,

    /// <summary>Binary file located at the url at "Source" property.</summary>
    BinaryFile,
}
