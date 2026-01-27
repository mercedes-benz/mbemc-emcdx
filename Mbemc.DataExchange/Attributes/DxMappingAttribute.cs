// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Marks a property saved as mapping at the storage.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class DxMappingAttribute : DxValueContentAttribute
{
    #region Public Properties

    /// <summary>Gets the variable names the mapping instance is bound to.</summary>
    public string[] VariableNames { get; set; } = [];

    #endregion Public Properties
}
