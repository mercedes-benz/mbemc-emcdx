// SPDX-License-Identifier: MIT
using System.Reflection;

namespace Mbemc.DataExchange;

/// <summary>Provides a variable</summary>
public record DxVariable : DxItemWithValues
{
    /// <summary>Creates an <see cref="DxVariable"/> instance from the specified <paramref name="property"/> and <paramref name="attribute"/> instances.</summary>
    /// <param name="property"></param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static DxVariable FromProperty(PropertyInfo property, DxAttribute? attribute = null)
    {
        attribute
            ??= property.GetCustomAttributes().OfType<DxAttribute>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Property {property} does not have an XChangeAttribute!");
        return new()
        {
            Name = attribute.Name ?? property.Name,
            Unit = attribute.Unit,
            Description = attribute.Description,
        };
    }

    /// <summary>Gets or sets a generator used generate the values for the variable. This is exclusive with <see cref="DxItemWithValues.Values"/> and <see cref="DxItemWithValues.BinaryValues"/>.</summary>
    public DxValueGeneratorSettings? Generator { get; init; }

    /// <summary>Gets the number of values present at the variable.</summary>
    /// <remarks>
    /// This is taken from <see cref="Generator"/>, <see cref="DxItemWithValues.Values"/> or <see cref="DxItemWithValues.BinaryValues"/>. Only one of the above
    /// instances shall be used at a time!
    /// </remarks>
    public override long ValueCount => Generator?.ValueCount ?? base.ValueCount;
}
