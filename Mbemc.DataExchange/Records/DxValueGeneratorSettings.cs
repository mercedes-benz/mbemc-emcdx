// SPDX-License-Identifier: MIT
using System.Diagnostics.CodeAnalysis;

namespace Mbemc.DataExchange;

/// <summary>Provides a value generator</summary>
public record DxValueGeneratorSettings : IEquatable<DxValueGeneratorSettings>
{
    [SuppressMessage("Style", "IDE0032", Justification = "Used for compatibility with format specification. Format requires length to allow scientific notation.")]
    long valueCount;

    /// <summary>Gets or sets the number of values to generate.</summary>
    public decimal Length
    {
        [Obsolete("Use ValueCount to get the number of values the generator will produce.")]
        get => valueCount;
        init => valueCount = (long)value;
    }

    /// <summary>Gets or sets the start (first) value the generator will return.</summary>
    public decimal Start { get; init; }

    /// <summary>Gets or sets the step (distance between two values) the generator will use.</summary>
    public decimal Step { get; init; }

    /// <summary>Gets or sets the generator mode.</summary>
    public DxValueGeneratorMode Mode { get; init; }

    /// <summary>Gets the number of values the generator will produce.</summary>
    [SuppressMessage("Style", "IDE0032", Justification = "Used for compatibility with format specification. Format requires length to allow scientific notation.")]
    public long ValueCount => valueCount;
}
