// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides an interface for named objects.</summary>
public interface IDxName
{
    #region Public Properties

    /// <summary>Gets or sets the name of the object.</summary>
    string? Name { get; }

    #endregion Public Properties
}
