// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides an interface for objects with unit.</summary>
public interface IDxUnit
{
    #region Public Properties

    /// <summary>Gets the unit of the values.</summary>
    string? Unit { get; }

    #endregion Public Properties
}
