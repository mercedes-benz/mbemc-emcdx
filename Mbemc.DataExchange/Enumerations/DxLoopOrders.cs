// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides available orders for sequences ( <see cref="DxLoopOrder"/>). <seealso href="https://en.wikipedia.org/wiki/Lexicographic_order"/>.</summary>
public static class DxLoopOrders
{
    #region Public Properties

    /// <summary>Lexicographic order <see cref="DxLoopOrder.Lex"/></summary>
    public static DxLoopOrder C => DxLoopOrder.Lex;

    /// <summary>Colexicographic order <see cref="DxLoopOrder.Colex"/></summary>
    public static DxLoopOrder ColexicographicalOrder => DxLoopOrder.Colex;

    /// <summary>Colexicographic order <see cref="DxLoopOrder.Colex"/></summary>
    public static DxLoopOrder Fortran => DxLoopOrder.Colex;

    /// <summary>Lexicographic order <see cref="DxLoopOrder.Lex"/></summary>
    public static DxLoopOrder LeftToRight => DxLoopOrder.Lex;

    /// <summary>Lexicographic order <see cref="DxLoopOrder.Lex"/></summary>
    public static DxLoopOrder LexicographicalOrder => DxLoopOrder.Lex;

    /// <summary>Colexicographic order <see cref="DxLoopOrder.Colex"/></summary>
    public static DxLoopOrder RightToLeft => DxLoopOrder.Colex;

    #endregion Public Properties
}
