// SPDX-License-Identifier: MIT
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Mbemc.DataExchange;

[DebuggerDisplay($"{nameof(DxPropertyWriter)} Name = {{Name}}")]
class DxPropertyWriter
{
    #region Private Fields

    /// <summary>Elements that can be used at the double array at the json element (compatible precision, never put long or ulong here!)</summary>
    static readonly Type[] allowedArrayElementTypes = [typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(float), typeof(double)];

    #endregion Private Fields

    #region Private Constructors

    DxPropertyWriter(DxItem item, DxDataPlacement placement, DxLoopOrder loopOrder, DxBinaryFormat binaryFormat, DxValueFormat valueFormat, DxSettings settings, string? binaryFileName, PropertyInfo? propertyInfo)
    {
        binaryFileName ??= Guid.NewGuid().ToString();
        Settings = settings;
        BinaryFormat = binaryFormat != 0 ? binaryFormat : Settings.BinaryFormat;
        ValueFormat = valueFormat != 0 ? valueFormat : Settings.ValueFormat;
        Placement = placement != 0 ? placement : Settings.Placement;
        LoopOrder = loopOrder != 0 ? loopOrder : Settings.LoopOrder;
        Item = item;
        PropertyInfo = propertyInfo;

        switch (Placement)
        {
            case DxDataPlacement.Array:
            case DxDataPlacement.Content:
            {
                FileName = null;
                break;
            }
            case DxDataPlacement.Autoselect:
            case DxDataPlacement.BinaryFile:
            {
                switch (BinaryFormat)
                {
                    case DxBinaryFormat.Text:
                    {
                        FileName = binaryFileName + ".txt";
                        break;
                    }
                    default:
                    {
                        FileName = binaryFileName + ".bin";
                        break;
                    }
                }
                break;
            }
            default:
            {
                throw new NotImplementedException($"ValueType {Placement} is not implemented!");
            }
        }
    }

    #endregion Private Constructors

    #region Private Properties

    DxBinaryValues? BinaryValues { get; set; }
    DxItem Item { get; }
    DxLoopOrder LoopOrder { get; }
    DxValueFormat ValueFormat { get; }
    IList? Values { get; set; }

    #endregion Private Properties

    #region Private Methods

    int[] CheckDimensions(int[] dimensions)
    {
        var expectedValueCount = 1;
        foreach (var dimension in dimensions)
        {
            if (dimension <= 0)
            {
                throw new InvalidDataException($"Invalid dimension value {dimension} at dimensions {string.Join(",", dimensions)}!");
            }
            expectedValueCount *= dimension;
        }
        return expectedValueCount == Values?.Count
            ? dimensions
            : throw new InvalidDataException($"Expected value count {expectedValueCount} with dimensions {string.Join(",", dimensions)} do not match present value count of {Values?.Count ?? 0}!");
    }

    byte[] GetBinary<TValue>(IList<TValue> list) where TValue : struct
    {
        if (list is not TValue[] array) array = [.. list];
        var data = new byte[array.Length * Marshal.SizeOf<TValue>()];
        Buffer.BlockCopy(array, 0, data, 0, data.Length);
        return data;
    }

    byte[]? GetBinary()
    {
        if (Values is null) return null;
        var contentType = Values.GetType();
        var valueFormat = DxExtensions.GetValueFormat(contentType);
        return valueFormat switch
        {
            DxValueFormat.Float64 => GetBinary((IList<double>)Values),
            DxValueFormat.Float32 => GetBinary((IList<float>)Values),
            DxValueFormat.Int8 => GetBinary((IList<sbyte>)Values),
            DxValueFormat.UInt8 => GetBinary((IList<byte>)Values),
            DxValueFormat.Int16 => GetBinary((IList<short>)Values),
            DxValueFormat.UInt16 => GetBinary((IList<ushort>)Values),
            DxValueFormat.Int32 => GetBinary((IList<int>)Values),
            DxValueFormat.UInt32 => GetBinary((IList<uint>)Values),
            DxValueFormat.Int64 => GetBinary((IList<long>)Values),
            DxValueFormat.UInt64 => GetBinary((IList<ulong>)Values),
            _ => throw new NotSupportedException($"Creating binary data for value format {valueFormat} is not supported!"),
        };
    }

    DxBinaryValues? GetBinaryValues(int[] dimensions, double offset = 0, double scale = 1)
    {
        if (BinaryValues is not null) throw new InvalidOperationException("This method can only be called once!");

        //no binary encoding of value arrays
        if (Placement is DxDataPlacement.Autoselect or DxDataPlacement.Array) return null;

        //no encoding of empty arrays
        var valueCount = Values?.Count ?? 0;
        if (valueCount == 0) return null;

        string? content = null;
        string? fileName = null;

        //set defaults
        var loopOrder = LoopOrder != 0 ? LoopOrder : DxLoopOrder.Lex;
        var binaryFormat = BinaryFormat != 0 ? BinaryFormat : DxBinaryFormat.Raw;
        var valueFormat = ValueFormat != 0 ? ValueFormat : Values!.Cast<object>().First().GetType().GetValueFormat();

        switch (Placement)
        {
            case DxDataPlacement.BinaryFile:
            {
                if (FileName is null) throw new InvalidOperationException("FileName is unset!");
                fileName = Path.GetFileName(WriteBinaryFile(FileName, binaryFormat));
                break;
            }
            case DxDataPlacement.Content:
            {
                switch (BinaryFormat)
                {
                    default: throw new NotSupportedException($"{nameof(BinaryFormat)} {BinaryFormat} is not supported at {nameof(Placement)} {Placement}!");
                    case DxBinaryFormat.Base64:
                    {
                        content = GetContentBase64(binaryFormat);
                        break;
                    }
                    case DxBinaryFormat.BinBlock:
                    {
                        var binary = GetBinary() ?? throw new NullReferenceException();
                        binary = DxExtensions.EncodeBinBlock(binary);
                        content = GetContentBase64(binaryFormat);
                        break;
                    }
                    case DxBinaryFormat.Text:
                    {
                        binaryFormat = DxBinaryFormat.Text;
                        content = GetContentText();
                        break;
                    }
                }
                break;
            }
            default:
            {
                throw new NotImplementedException();
            }
        }

        return BinaryValues = new DxBinaryValues()
        {
            ByteOrder = BitConverter.IsLittleEndian ? DxEndian.LittleEndian : DxEndian.BigEndian,
            Dimensions = CheckDimensions(dimensions),
            Format = valueFormat,
            LoopOrder = loopOrder,
            Offset = offset,
            Scale = scale,
            SourceFormat = binaryFormat,
            ValueCount = valueCount,
            Content = content,
            Source = fileName,
        };
    }

    string GetContentBase64<TValue>(IList<TValue> list, DxBinaryFormat format) where TValue : struct
    {
        if (!(Values?.Count > 0)) return string.Empty;
        byte[] data;
        switch (format)
        {
            case DxBinaryFormat.BinBlock:
            {
                data = GetBinary(list);
                data = DxExtensions.EncodeBinBlock(data);
                break;
            }
            case DxBinaryFormat.Base64:
            {
                data = GetBinary(list);
                break;
            }
            default: throw new NotSupportedException($"Base64 encoded {format} is not supported at content field!");
        }

        return DxExtensions.EncodeBase64(data);
    }

    string? GetContentBase64(DxBinaryFormat format)
    {
        if (Values is null) return null;
        var contentType = Values.GetType();
        var valueFormat = DxExtensions.GetValueFormat(contentType);
        return valueFormat switch
        {
            DxValueFormat.Float64 => GetContentBase64((IList<double>)Values, format),
            DxValueFormat.Float32 => GetContentBase64((IList<float>)Values, format),
            DxValueFormat.Int8 => GetContentBase64((IList<sbyte>)Values, format),
            DxValueFormat.UInt8 => GetContentBase64((IList<byte>)Values, format),
            DxValueFormat.Int16 => GetContentBase64((IList<short>)Values, format),
            DxValueFormat.UInt16 => GetContentBase64((IList<ushort>)Values, format),
            DxValueFormat.Int32 => GetContentBase64((IList<int>)Values, format),
            DxValueFormat.UInt32 => GetContentBase64((IList<uint>)Values, format),
            DxValueFormat.Int64 => GetContentBase64((IList<long>)Values, format),
            DxValueFormat.UInt64 => GetContentBase64((IList<ulong>)Values, format),
            _ => throw new NotSupportedException($"Creating embedded content for value format {valueFormat} is not supported!"),
        };
    }

    string? GetContentText()
    {
        if (Values is null) return null;
        var contentType = Values.GetType();
        var valueFormat = DxExtensions.GetValueFormat(contentType);
        return valueFormat switch
        {
            DxValueFormat.Float64 => GetContentText((IList<double>)Values),
            DxValueFormat.Float32 => GetContentText((IList<float>)Values),
            DxValueFormat.Int8 => GetContentText((IList<sbyte>)Values),
            DxValueFormat.UInt8 => GetContentText((IList<byte>)Values),
            DxValueFormat.Int16 => GetContentText((IList<short>)Values),
            DxValueFormat.UInt16 => GetContentText((IList<ushort>)Values),
            DxValueFormat.Int32 => GetContentText((IList<int>)Values),
            DxValueFormat.UInt32 => GetContentText((IList<uint>)Values),
            DxValueFormat.Int64 => GetContentText((IList<long>)Values),
            DxValueFormat.UInt64 => GetContentText((IList<ulong>)Values),
            _ => throw new NotSupportedException($"Creating embedded content for value format {valueFormat} is not supported!"),
        };
    }

    string GetContentText<TValue>(IEnumerable<TValue> values) where TValue : struct, IFormattable
    {
        StringBuilder sb = new();
        foreach (var value in values)
        {
            if (value is float or double)
            {
                sb.Append(value.ToString("R", CultureInfo.InvariantCulture));
            }
            else
            {
                sb.Append(value.ToString());
            }

            sb.Append(';');
        }
        return sb.ToString();
    }

    IList<TValue>? GetValueArray<TValue>()
    {
        if (Values is null || (Placement != DxDataPlacement.Array && Placement != DxDataPlacement.Autoselect)) return null;
        if (Values is IList<TValue> list)
        {
            return list;
        }
        if (Values is IEnumerable)
        {
            list = new List<TValue>();
            foreach (var value in Values)
            {
                list.Add((TValue)Convert.ChangeType(value, typeof(TValue)));
            }
            return list;
        }
        throw new InvalidDataException($"Cannot convert values array to IList<{typeof(TValue).Name}>!");
    }

    void SourceTypeUpdate(string reason)
    {
        Placement = Settings.Placement == DxDataPlacement.Autoselect ? DxDataPlacement.BinaryFile : Settings.Placement;
        Trace.WriteLine($"Change {nameof(Placement)} to {Placement}. {reason}");
    }

    string? WriteBinaryFile(string fileName, DxBinaryFormat format)
    {
        if (Values?.Count > 0)
        {
            using var stream = Settings.CreateFile(fileName);
            switch (format)
            {
                case DxBinaryFormat.BinBlock:
                {
                    var data = GetBinary() ?? throw new NullReferenceException();
                    if (data.Length > Settings.MaxBinFileSize)
                    {
                        throw new ArgumentOutOfRangeException($"Length exceeds the maximum of {Settings.MaxBinFileSize}.");
                    }
                    stream.WriteBinBlock(data);
                    break;
                }
                case DxBinaryFormat.Raw:
                {
                    var data = GetBinary() ?? throw new NullReferenceException();
                    if (data.Length > Settings.MaxBinFileSize)
                    {
                        throw new ArgumentOutOfRangeException($"Length exceeds the maximum of {Settings.MaxBinFileSize}.");
                    }
                    stream.Write(data, 0, data.Length);
                    break;
                }
                case DxBinaryFormat.Text:
                {
                    var writer = new StreamWriter(stream, Encoding.UTF8);
                    var valuesAsText = GetContentText() ?? throw new NullReferenceException();
                    var splittedValues = valuesAsText.Split(';');
                    // each value is printed in a new line. Therefore we have to add the length of the newline
                    if (splittedValues.Length * Environment.NewLine.Length > Settings.MaxBinFileSize)
                    {
                        throw new ArgumentOutOfRangeException($"Length exceeds the maximum of {Settings.MaxBinFileSize}.");
                    }
                    foreach (var value in splittedValues)
                    {
                        writer.WriteLine(value);
                    }
                    writer.Close();
                    break;
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
            stream.Close();
            return stream.Name;
        }
        return null;
    }

    #endregion Private Methods

    #region Public Properties

    public DxBinaryFormat BinaryFormat { get; private set; }

    public string? FileName { get; }

    public bool IsMapping => Item is DxMapping;

    public bool IsVariable => Item is DxVariable;

    public string Name => Item.Name;

    public DxDataPlacement Placement { get; private set; }

    public PropertyInfo? PropertyInfo { get; }

    public DxSettings Settings { get; }

    public bool UseFile => Placement == DxDataPlacement.BinaryFile && FileName is not null;

    public long ValueCount => BinaryValues?.ValueCount ?? Values?.Count ?? 0;

    #endregion Public Properties

    #region Public Methods

    public static DxPropertyWriter Create(DxSettings settings, DxMapping mapping, DxDataPlacement placement = 0, DxLoopOrder loopOrder = 0, DxBinaryFormat binaryFormat = 0, DxValueFormat valueFormat = 0, string? binaryFileName = null, PropertyInfo? propertyInfo = null)
        => new(mapping, placement, loopOrder, binaryFormat, valueFormat, settings, binaryFileName, propertyInfo);

    public static DxPropertyWriter Create(DxSettings settings, DxVariable variable, DxDataPlacement placement = 0, DxLoopOrder loopOrder = 0, DxBinaryFormat binaryFormat = 0, DxValueFormat valueFormat = 0, string? binaryFileName = null, PropertyInfo? propertyInfo = null)
        => new(variable, placement, loopOrder, binaryFormat, valueFormat, settings, binaryFileName, propertyInfo);

    public static DxPropertyWriter Create(DxSettings settings, PropertyInfo propertyInfo, string? binaryFileName = null)
    {
        binaryFileName ??= $"{propertyInfo.DeclaringType!.Name}.{propertyInfo.Name}";

        if (propertyInfo.GetCustomAttribute<DxMappingAttribute>() is DxMappingAttribute mapping)
        {
            var item = new DxMapping()
            {
                VariableNames = mapping.VariableNames,
                Name = mapping.Name ?? propertyInfo.Name,
                Description = mapping.Description,
                Unit = mapping.Unit
            };
            return Create(settings, item, binaryFileName: binaryFileName, placement: mapping.Placement, loopOrder: mapping.LoopOrder, binaryFormat: mapping.BinaryFormat, valueFormat: mapping.ValueFormat, propertyInfo: propertyInfo);
        }

        if (propertyInfo.GetCustomAttribute<DxVariableAttribute>() is DxVariableAttribute variable)
        {
            var item = new DxVariable()
            {
                Name = variable.Name ?? propertyInfo.Name,
                Description = variable.Description,
                Unit = variable.Unit
            };
            return Create(settings, item, binaryFileName: binaryFileName, placement: variable.Placement, loopOrder: variable.LoopOrder, binaryFormat: variable.BinaryFormat, valueFormat: variable.ValueFormat, propertyInfo: propertyInfo);
        }

        throw new ArgumentException($"Property {propertyInfo.Name} is not a [Variable] or [Mapping]!", nameof(propertyInfo));
    }

    public void Add<TValue>(TValue value)
            where TValue : struct
    {
        if (Placement == DxDataPlacement.Autoselect)
        {
            if (Values is not null && Values.Count >= Settings.ValueCountLimit)
            {
                SourceTypeUpdate($"Value count exceeds {Settings.ValueCountLimit} items!");
            }
            else
            {
                var elementType = typeof(TValue);
                if (!allowedArrayElementTypes.Contains(elementType))
                {
                    SourceTypeUpdate($"Preventing precision loss of {elementType} to double conversion!");
                }
            }
        }

        if (Values is not IList<TValue> list)
        {
            if (Values is not null) throw new InvalidOperationException($"Cannot add value of type {typeof(TValue)} to value list of type {Values.GetType()}");
            list = new List<TValue>();
            Values = (IList)list;
        }
        list.Add(value);
    }

    public void AddContent(object content)
    {
        var contentType = content.GetType();
        var valueFormat = DxExtensions.GetValueFormat(contentType);
        switch (valueFormat)
        {
            case DxValueFormat.Float64: Add((double)content); return;
            case DxValueFormat.Float32: Add((float)content); return;
            case DxValueFormat.Int8: Add((sbyte)content); return;
            case DxValueFormat.UInt8: Add((byte)content); return;
            case DxValueFormat.Int16: Add((short)content); return;
            case DxValueFormat.UInt16: Add((ushort)content); return;
            case DxValueFormat.Int32: Add((int)content); return;
            case DxValueFormat.UInt32: Add((uint)content); return;
            case DxValueFormat.Int64: Add((long)content); return;
            case DxValueFormat.UInt64: Add((ulong)content); return;
            default: throw new NotSupportedException($"Writing content {content} of type {content.GetType()} with value format {valueFormat}) is not supported!");
        }
    }

    public void AddRange<TDataset>(IEnumerable<TDataset> values)
            where TDataset : struct
    {
        foreach (var value in values)
        {
            Add(value);
        }
    }

    public DxMapping GetMapping() => new()
    {
        Name = Name,
        Description = Item?.Description,
        Unit = Item?.Unit,
        Values = GetValueArray<double>(),
        BinaryValues = GetBinaryValues([Values?.Count ?? 0]),
        VariableNames = (Item as DxMapping)?.VariableNames ?? [],
    };

    public DxVariable GetVariable() => new()
    {
        Name = Name,
        Description = Item?.Description,
        Unit = Item?.Unit,
        Values = GetValueArray<double>(),
        BinaryValues = GetBinaryValues([Values?.Count ?? 0]),
    };

    public void SetContent(object content, bool throwIfAlreadySet)
    {
        if (throwIfAlreadySet && Values?.Count > 0)
        {
            throw new InvalidOperationException("Content was already set!");
        }

        //this matches array too
        if (content is IList list)
        {
            if (Placement == DxDataPlacement.Autoselect)
            {
                if (list.Count >= Settings.ValueCountLimit)
                {
                    SourceTypeUpdate($"Value count exceeds {Settings.ValueCountLimit} items!");
                }
                else
                {
                    var elementType = DxExtensions.GetElementType(content);
                    if (!allowedArrayElementTypes.Contains(elementType))
                    {
                        SourceTypeUpdate($"Preventing precision loss of {elementType} to double conversion!");
                    }
                }
            }
            Values = list;
            return;
        }
        AddContent(content);
    }

    #endregion Public Methods
}
