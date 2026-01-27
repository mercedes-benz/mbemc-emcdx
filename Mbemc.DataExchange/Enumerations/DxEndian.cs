// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides the endianness (order of bytes of a word) at the storage.</summary>
public enum DxEndian : uint
{
    /// <summary>Undefined. This cannot be interpreted.</summary>
    Undefined = 0,

    /// <summary>Big-endian stores the most significant byte of a word at the smallest memory address and the least significant byte at the largest.</summary>
    BigEndian,

    /// <summary>Little-endian stores the least-significant byte at the smallest memory address and the most significant byte at the largest.</summary>
    LittleEndian
}
