// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides an interface for commented objects.</summary>
public interface IDxDescription
{
    #region Public Properties

    /// <summary>Gets or sets the name of the object.</summary>
    public string? Description { get; }

    #endregion Public Properties
}
