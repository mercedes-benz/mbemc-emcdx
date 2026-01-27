// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides value generator modes</summary>
public enum DxValueGeneratorMode
{
    /// <summary>Undefined value generator mode.</summary>
    Undefined = 0,

    /// <summary>Generator generates values with linear distance.</summary>
    Linear,

    /// <summary>Generator generates values with logarithmic distance.</summary>
    Logarithmic
}
