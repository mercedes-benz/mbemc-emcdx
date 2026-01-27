// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides dictionary and sequence ordering modes.</summary>
public enum DxLoopOrder : uint
{
    /// <summary>Undefined ordering. The values in this collection cannot be interpreted.</summary>
    Undefined = 0,

    /// <summary>
    /// Alphabetical order of the dictionaries to sequences of ordered symbols or, more generally, of elements of a totally ordered set. Lexicographical order
    /// is obtained by reading finite sequences from the left to the right.
    /// </summary>
    Lex = 1,

    /// <summary>
    /// The colexicographic or colex order is a variant of the lexicographical order that is obtained by reading finite sequences from the right to the left
    /// instead of reading them from the left to the right.
    /// </summary>
    Colex = 2
}
