// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides an internally used non generic interface for the <see cref="DxDataWriter"/> classes.</summary>
public interface IDxDataWriter
{
    #region Public Properties

    /// <summary>Gets the file names used by this writer.</summary>
    IList<string> FileNames { get; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Closes the data writer, all streams but still allows to get the result via <see cref="ToDataItem()"/>.</summary>
    void Close();

    /// <summary>Generates the resulting <see cref="DxDataItem"/>.</summary>
    /// <returns>Returns a new instance of the <see cref="DxDataItem"/> record.</returns>
    DxDataItem ToDataItem();

    #endregion Public Methods
}
