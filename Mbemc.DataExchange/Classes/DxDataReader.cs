// SPDX-License-Identifier: MIT
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mbemc.DataExchange;

/// <summary>Provides a reader class able to read a <see cref="DxDataItem"/></summary>
/// <typeparam name="TDataset">Record to read.</typeparam>
[DebuggerDisplay("DxDataReader Name = {Name}")]
public class DxDataReader<TDataset>
    where TDataset : class, new()
{
    #region Private Fields

    readonly DxReader reader;

    #endregion Private Fields

    #region Private Methods

    static void SetArray<TElement>(PropertyInfo property, TDataset result, IList<TElement> values) where TElement : struct
    {
        try
        {
            var elementType = property.PropertyType.GetElementType() ?? throw new InvalidOperationException("Array element type is null!");
            if (elementType == typeof(double))
            {
                property.SetValue(result, values);
            }
            else
            {
                //convert array elements to target type
                var array = Array.CreateInstance(elementType, values.Count);
                for (var i = 0; i < values.Count; i++)
                {
                    var targetValue = Convert.ChangeType(values[i], elementType);
                    array.SetValue(targetValue, i);
                }
                property.SetValue(result, array);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not set values to property {property} at type {typeof(TDataset)}!", ex);
        }
    }

    static void SetList<TElement>(PropertyInfo property, TDataset result, IList<TElement> values) where TElement : struct
    {
        try
        {
            var elementType = property.PropertyType.GetGenericArguments().Single();
            if (elementType == typeof(double))
            {
                property.SetValue(result, values);
            }
            else
            {
                //convert array elements to target type
                var array = Array.CreateInstance(elementType, values.Count);
                for (var i = 0; i < values.Count; i++)
                {
                    var targetValue = Convert.ChangeType(values[i], elementType);
                    array.SetValue(targetValue, i);
                }
                property.SetValue(result, array);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not set values to property {property} at type {typeof(TDataset)}!", ex);
        }
    }

    static void SetProperty<TElement>(PropertyInfo property, TDataset result, IList<TElement> list) where TElement : struct
    {
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
        {
            SetList(property, result, list);
            return;
        }
        if (property.PropertyType.IsArray)
        {
            var array = list as TElement[] ?? list.ToArray() ?? throw new InvalidOperationException("Could not convert to array!");
            SetArray(property, result, array);
            return;
        }
        if (list.Count == 1)
        {
            SetValue(property, result, list.Single());
            return;
        }
        throw new InvalidOperationException($"Can not determine how to load IList<double> into property of type {property.PropertyType}!");
    }

    static void SetValue(PropertyInfo property, TDataset result, object? content)
    {
        try
        {
            var targetValue = content is null ? null : property.PropertyType.IsAssignableFrom(content.GetType()) ? content : Convert.ChangeType(content, property.PropertyType);
            property.SetValue(result, targetValue);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not set {content} to property {property} at type {typeof(TDataset)}!", ex);
        }
    }

    static Exception ThrowLoadValues(string name) => new InvalidOperationException($"Could not load values for {name}!");

    void LoadVariable(DxVariable variable, PropertyInfo property, TDataset result)
    {
        if (variable.Values is not null)
        {
            SetProperty(property, result, variable.Values);
            return;
        }

        if (variable.BinaryValues is DxBinaryValues binary)
        {
            switch (binary.Format)
            {
                case DxValueFormat.Int8:
                {
                    SetProperty(property, result, Settings.LoadValues<sbyte>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.UInt8:
                {
                    SetProperty(property, result, Settings.LoadValues<byte>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.Int16:
                {
                    SetProperty(property, result, Settings.LoadValues<short>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.UInt16:
                {
                    SetProperty(property, result, Settings.LoadValues<ushort>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.Int32:
                {
                    SetProperty(property, result, Settings.LoadValues<int>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.UInt32:
                {
                    SetProperty(property, result, Settings.LoadValues<uint>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.Int64:
                {
                    SetProperty(property, result, Settings.LoadValues<long>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.UInt64:
                {
                    SetProperty(property, result, Settings.LoadValues<ulong>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.Float32:
                {
                    SetProperty(property, result, Settings.LoadValues<float>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                case DxValueFormat.Float64:
                {
                    SetProperty(property, result, Settings.LoadValues<double>(binary) ?? throw ThrowLoadValues(variable.Name));
                    return;
                }
                default:
                {
                    throw new NotImplementedException($"{nameof(DxValueFormat)}.{binary.Format} is not implemented!");
                }
            }
        }

        Trace.TraceWarning($"Variable {variable} contains no values!");
    }

    TDataset? Read(DxDataItem item)
    {
        var loaded = 0;
        TDataset result = new();
        foreach (var property in Properties)
        {
            if (property.Name == nameof(item.Name))
            {
                SetValue(property, result, item.Name);
                continue;
            }

            if (property.Name == nameof(item.Description))
            {
                SetValue(property, result, item.Description);
                continue;
            }

            foreach (var attribute in property.GetCustomAttributes())
            {
                if (attribute is not DxAttribute) continue;
                if (attribute is DxGeneratedAttribute generator)
                {
                    loaded++;
                    throw new NotImplementedException();
                }
                if (attribute is DxMappingAttribute mapping)
                {
                    var itemName = mapping.Name ?? property.Name;
                    loaded++;
                    throw new NotImplementedException();
                }
                if (attribute is DxVariableAttribute variableAttribute && TryGetItem(item.Variables, property, variableAttribute, out var variable))
                {
                    loaded++;
                    LoadVariable(variable, property, result);
                    continue;
                }
                Trace.TraceWarning($"Ignore property {property}");
            }
        }

        var expected = item.Mappings.Count + item.Variables.Count;
        if (loaded < expected)
        {
            Trace.WriteLine($"Properties read: {loaded}, expected: {expected}!");
        }

        return loaded > 0 ? result : null;
    }

    bool TryGetItem<TResult>(IEnumerable<TResult> list, PropertyInfo property, DxAttribute attribute, [MaybeNullWhen(false)] out TResult result)
        where TResult : class, IDxName, IDxUnit
    {
        foreach (var item in list)
        {
            if (item.Name == property.Name || item.Name == attribute.Name)
            {
                if (!Equals(item.Unit, attribute.Unit)) throw new InvalidOperationException($"Unit in file {item.Unit} does not match definition {attribute.Unit} at property {property} ({property.DeclaringType})!");
                result = item;
                return true;
            }
        }
        result = null;
        return false;
    }

    #endregion Private Methods

    #region Internal Constructors

    internal DxDataReader(DxReader reader)
    {
        this.reader = reader;
        var rowType = typeof(TDataset);
        Properties = rowType.GetProperties();
    }

    #endregion Internal Constructors

    #region Public Properties

    /// <summary>Gets the properties of <typeparamref name="TDataset"/>.</summary>
    public PropertyInfo[] Properties { get; }

    /// <summary>Gets the settings used by the reader.</summary>
    public DxSettings Settings => reader.Settings;

    #endregion Public Properties

    #region Public Methods

    /// <summary>Reads all data items matching <typeparamref name="TDataset"/>.</summary>
    /// <returns>Returns an <see cref="IEnumerable{TDataset}"/>.</returns>
    public IEnumerable<TDataset> Read(Func<DxDataItem, bool>? predicate = null)
    {
        IEnumerable<DxDataItem> items = reader.File.Data;
        if (predicate is not null) items = items.Where(predicate);
        foreach (var item in items)
        {
            var result = Read(item);
            if (result is not null)
            {
                yield return result;
            }
        }
    }

    #endregion Public Methods
}
