// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Marks a property as generated at storage.</summary>
/// <remarks>Properties marked with this attribute will not be saved, instead the generator settings will be saved.</remarks>
[AttributeUsage(AttributeTargets.Property)]
public class DxGeneratedAttribute : DxAttribute
{
}
