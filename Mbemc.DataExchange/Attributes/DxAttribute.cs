// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides a base class for all attributes.</summary>
public abstract class DxAttribute : Attribute, IDxName, IDxDescription, IDxUnit
{
    #region Public Properties

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Unit { get; set; }

    #endregion Public Properties
}
