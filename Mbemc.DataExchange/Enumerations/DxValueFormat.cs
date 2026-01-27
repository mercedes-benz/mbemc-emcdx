// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides value formats and sizes.</summary>
public enum DxValueFormat : uint
{
    /// <summary>Undefined value format and size.</summary>
    Undefined = 0,

    // one byte types

    /// <summary>8 bit signed integer.</summary>
    Int8 = 1 << 8,

    /// <summary>8 bit unsigned integer.</summary>
    UInt8,

    // two byte types

    /// <summary>Half precision IEEE_754 value. <see href="https://en.wikipedia.org/wiki/Half-precision_floating-point_format"/></summary>
    Float16 = 2 << 8,

    /// <summary>16 bit signed integer.</summary>
    Int16,

    /// <summary>16 bit unsigned integer.</summary>
    UInt16,

    // three byte types

    /// <summary>24 bit signed integer.</summary>
    Int24 = 3 << 8,

    /// <summary>24 bit unsigned integer.</summary>
    UInt24,

    // four byte types

    /// <summary>Single precision IEEE_754 value. <see href="https://en.wikipedia.org/wiki/Single-precision_floating-point_format"/></summary>
    Float32 = 4 << 8,

    /// <summary>32 bit signed integer.</summary>
    Int32,

    /// <summary>32 bit unsigned integer.</summary>
    UInt32,

    // 8 byte types

    /// <summary>Double precision IEEE_754 value. <see href="https://en.wikipedia.org/wiki/Double-precision_floating-point_format"/></summary>
    Float64 = 8 << 8,

    /// <summary>64 bit signed integer.</summary>
    Int64,

    /// <summary>64 bit unsigned integer.</summary>
    UInt64,

    // 16 byte types

    /// <summary>Quadruple precision IEEE_754 value. <see href="https://en.wikipedia.org/wiki/Quadruple-precision_floating-point_format"/></summary>
    Float128 = 16 << 8,

    // 32 byte types

    /// <summary>Octuple precision IEEE_754 value. <see href="https://en.wikipedia.org/wiki/Octuple-precision_floating-point_format"/></summary>
    Float256 = 32 << 8,
}
