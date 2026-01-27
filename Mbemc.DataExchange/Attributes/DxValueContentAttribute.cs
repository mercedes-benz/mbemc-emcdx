// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Base attribute to allow settings for <see cref="DxItemWithValues.Values"/> and/or <see cref="DxItemWithValues.BinaryValues"/>.</summary>
public abstract class DxValueContentAttribute : DxAttribute
{
    #region Public Properties

    /// <summary>Gets the <see cref="DxBinaryFormat"/> to use for the variable instance.</summary>
    /// <remarks>This allows to control the storage format of the binary block containing all values.</remarks>
    public DxBinaryFormat BinaryFormat { get; set; }

    /// <summary>Gets the <see cref="DxLoopOrder"/> to use for the variable instance.</summary>
    /// <remarks>This allows to control how multi dimensional arrays, lists and matrices are stored.</remarks>
    public DxLoopOrder LoopOrder { get; set; }

    /// <summary>Gets the <see cref="DxDataPlacement"/> to use for the variable instance.</summary>
    /// <remarks>This allows to control the placement of the underlying value array.</remarks>
    public DxDataPlacement Placement { get; set; }

    /// <summary>Gets the <see cref="DxValueFormat"/> to use for the variable instance.</summary>
    /// <remarks>This allows to control the storage format of each value. This requires <see cref="Placement"/> to be set to <see cref="DxDataPlacement.BinaryFile"/>.</remarks>
    public DxValueFormat ValueFormat { get; set; }

    #endregion Public Properties
}
