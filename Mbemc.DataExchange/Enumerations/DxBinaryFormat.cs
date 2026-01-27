// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides available binary formats</summary>
public enum DxBinaryFormat : uint
{
    /// <summary>Undefined source format. This cannot be written / read.</summary>
    Undefined = 0,

    /// <summary>Raw external data saved to the uri defined at <see cref="DxBinaryValues.Source"/>.</summary>
    Raw,

    /// <summary>External data saved as text with one line per value to the uri defined at <see cref="DxBinaryValues.Source"/> using international formatting.</summary>
    Text,

    /// <summary>Visa bin block with mbn 50284-2 2022 extensions.</summary>
    BinBlock,

    /// <summary>Base 64 encoded data at content field.</summary>
    Base64,
}
