// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides a mapping of variables to values.</summary>
public record DxMapping : DxItemWithValues
{
    /// <summary>Gets or sets an array with variable names.</summary>
    public IList<string> VariableNames { get; init; } = [];
}
