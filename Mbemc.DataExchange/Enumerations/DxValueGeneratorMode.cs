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
    /// <remarks>Values grow exponentially using a logarithmic growth factor: value = start * e^(stepSize * step)</remarks>
    Logarithmic,

    /// <summary>Generator generates values with percent distance.</summary>
    /// <remarks>Values grow by a fixed percentage per step: value = start * (1 + percent / 100)^step</remarks>
    Percentual,
}
