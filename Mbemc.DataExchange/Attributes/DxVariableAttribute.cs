// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Marks a property saved as variable at the storage.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class DxVariableAttribute : DxValueContentAttribute
{
}
