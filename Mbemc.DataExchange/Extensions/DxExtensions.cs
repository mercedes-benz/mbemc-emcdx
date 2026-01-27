// SPDX-License-Identifier: MIT
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Mbemc.DataExchange;

/// <summary>Provides extensions for binary writing and reading.</summary>
public static class DxExtensions
{
    #region Private Fields

    static readonly char[] InvalidCharacters = [.. Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct()];
    static DxEndian? machineType;

    #endregion Private Fields

    #region Private Methods

    static IList<TValue> DecodeTextValues<TValue>(IEnumerable<string> content)
            where TValue : struct
    {
        var parse = typeof(TValue).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string), typeof(IFormatProvider)], null)
            ?? throw new InvalidOperationException($"Type {typeof(TValue)} has no static Parse(string, IFormatProvider) function!");
        var values = new List<TValue>();
        var emptyItem = false;
        foreach (var str in content)
        {
            if (emptyItem) throw new InvalidOperationException("Empty value in content!");
            if (str == string.Empty) { emptyItem = true; continue; }
            var result = parse.Invoke(null, [str, CultureInfo.InvariantCulture]);
            if (result is TValue value) values.Add(value);
        }
        return values;
    }

    [ExcludeFromCodeCoverage]
    static DxEndian GetMachineType()
    {
        var byteArray = new byte[8] { 18, 52, 86, 120, 154, 188, 222, 240 };
        var longArray = new ulong[1] { 0 };
        Buffer.BlockCopy(byteArray, 0, longArray, 0, 8);
        return longArray[0] switch
        {
            17356517385562371090uL => DxEndian.LittleEndian,
            1311768467463790320uL => DxEndian.BigEndian,
            _ => DxEndian.Undefined,
        };
    }

    static IList<TValue> LoadBinaryValuesContent<TValue>(this DxBinaryValues bin)
            where TValue : struct
    {
        var content = bin.Content ?? throw new ArgumentException("BinaryValues.Content need to be set!");
        switch (bin.SourceFormat)
        {
            default:
            {
                throw new NotSupportedException($"XChangeSourceFormat.{bin.SourceFormat} is not supported at XChangeBinaryValues.Content!");
            }
            case DxBinaryFormat.Text:
            {
                return DecodeTextValues<TValue>(content.Split(';'));
            }
            case DxBinaryFormat.Base64:
            {
                var block = DecodeBase64(content);
                return ConvertBinaryValues<TValue>(bin, block);
            }
            case DxBinaryFormat.BinBlock:
            {
                var block = DecodeBase64(content);
                block = DecodeBinBlock(block);
                return ConvertBinaryValues<TValue>(bin, block);
            }
        }
    }

    static byte[] Swap(byte[] data, int bytes)
    {
        // taken from Cave.IO.Endian.Swap
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (bytes < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes));
        }

        var array = new byte[data.Length];
        bytes--;
        var sourceArrayIndex = 0;
        while (sourceArrayIndex < data.Length)
        {
            var targetArrayIndex = sourceArrayIndex + bytes;
            var numBytesSwapped = 0;
            while (numBytesSwapped <= bytes)
            {
                array[targetArrayIndex] = data[sourceArrayIndex];
                numBytesSwapped++;
                sourceArrayIndex++;
                targetArrayIndex--;
            }
        }

        return array;
    }

    #endregion Private Methods

    #region Internal Properties

    internal static DxEndian MachineType => machineType ??= GetMachineType();

    #endregion Internal Properties

    #region Public Properties

    /// <summary>Allow the deserializer to read a value when expecting an array.</summary>
    /// <remarks>
    /// This is needed because the default matlab serializer does not write arrays with a single element. Matlab writes a single element instead of an array
    /// with one element.
    /// </remarks>
    public static bool EnableMatlabArrayExtensions { get; set; } = true;

    #endregion Public Properties

    #region Public Methods

    /// <summary>
    /// Converts the specified byte array to a list of values. All settings ( <see cref="DxBinaryValues.Format"/>, <see cref="DxBinaryValues.LoopOrder"/>,
    /// <see cref="DxBinaryValues.Scale"/>, ...) will be incorporated.
    /// </summary>
    /// <param name="bin">BinaryValues instance</param>
    /// <param name="data">Raw data or encoded data block</param>
    /// <returns>Returns a typed list of values.</returns>
    /// <exception cref="ArgumentException">BinaryValues.Content need to be set!</exception>
    /// <exception cref="NotImplementedException">BytesPerValue {BytesPerValue} is not implemented!</exception>
    /// <exception cref="NotImplementedException">XChangeValueFormat.{Format} is not implemented!</exception>
    public static IList<TValue> ConvertBinaryValues<TValue>(this DxBinaryValues bin, byte[] data)
        where TValue : struct
    {
        var dataCopy = (byte[])data.Clone();
        switch (bin.BytesPerValue)
        {
            case 1: break;
            case 2:
            case 4:
            case 8:
            {
                if (bin.ByteOrder != MachineType)
                {
                    dataCopy = Swap(dataCopy, bin.BytesPerValue);
                }
                break;
            }
            default: throw new NotSupportedException($"BytesPerValue {bin.BytesPerValue} is not implemented!");
        }

        if (bin.LoopOrder == DxLoopOrder.Colex && bin.IsMulti) throw new NotImplementedException("DxLoopOrder.Colex with multidimensional data is not implemented yet!");

        switch (bin.Format)
        {
            case DxValueFormat.Int8:
            {
                if (typeof(TValue) != typeof(sbyte)) throw new InvalidOperationException($"Cannot convert values from {typeof(sbyte)} to {typeof(TValue)}!");
                var values = new sbyte[dataCopy.Length];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.Int16:
            {
                if (typeof(TValue) != typeof(short)) throw new InvalidOperationException($"Cannot convert values from {typeof(short)} to {typeof(TValue)}!");
                var values = new short[dataCopy.Length / 2];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.Int32:
            {
                if (typeof(TValue) != typeof(int)) throw new InvalidOperationException($"Cannot convert values from {typeof(int)} to {typeof(TValue)}!");
                var values = new int[dataCopy.Length / 4];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.Int64:
            {
                if (typeof(TValue) != typeof(long)) throw new InvalidOperationException($"Cannot convert values from {typeof(long)} to {typeof(TValue)}!");
                var values = new long[dataCopy.Length / 8];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.UInt8:
            {
                if (typeof(TValue) != typeof(byte)) throw new InvalidOperationException($"Cannot convert values from {typeof(byte)} to {typeof(TValue)}!");
                var values = (byte[])dataCopy.Clone();
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.UInt16:
            {
                if (typeof(TValue) != typeof(ushort)) throw new InvalidOperationException($"Cannot convert values from {typeof(ushort)} to {typeof(TValue)}!");
                var values = new ushort[dataCopy.Length / 2];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.UInt32:
            {
                if (typeof(TValue) != typeof(uint)) throw new InvalidOperationException($"Cannot convert values from {typeof(uint)} to {typeof(TValue)}!");
                var values = new uint[dataCopy.Length / 4];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.UInt64:
            {
                if (typeof(TValue) != typeof(ulong)) throw new InvalidOperationException($"Cannot convert values from {typeof(ulong)} to {typeof(TValue)}!");
                var values = new ulong[dataCopy.Length / 8];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.Float32:
            {
                if (typeof(TValue) != typeof(float)) throw new InvalidOperationException($"Cannot convert values from {typeof(float)} to {typeof(TValue)}!");
                var values = new float[dataCopy.Length / 4];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            case DxValueFormat.Float64:
            {
                if (typeof(TValue) != typeof(double)) throw new InvalidOperationException($"Cannot convert values from {typeof(double)} to {typeof(TValue)}!");
                var values = new double[dataCopy.Length / 8];
                Buffer.BlockCopy(dataCopy, 0, values, 0, dataCopy.Length);
                Scale(values, bin.Scale, bin.Offset);
                return (IList<TValue>)(object)values;
            }
            default: throw new NotImplementedException($"XChangeValueFormat.{bin.Format} is not implemented!");
        }
    }

    /// <summary>Creates a file name from the specified title.</summary>
    /// <param name="title">Title</param>
    /// <returns>Returns a filename containing only characters in range [A..Za..z0..9-]</returns>
    public static string CreateFileName(string title)
    {
        StringBuilder result = new();
        var last = ' ';
        for (var i = 0; i < title.Length; i++)
        {
            var ch = title[i];
            if (ch is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9'))
            {
                // valid characters A..Z, a..z, 0..9
            }
            else
            {
                ch = '-';
                // no two dashes in a row
                if (last == '-') continue;
            }
            last = ch;
            result.Append(ch);
        }
        return result.ToString().Trim('-');
    }

    /// <summary>Decodes a base 64 string.</summary>
    /// <param name="text">String to read.</param>
    /// <returns>Returns a new byte array.</returns>
    public static byte[] DecodeBase64(this string text) => Convert.FromBase64String(text);

    /// <summary>Decodes a bin block.</summary>
    /// <param name="binBlock">Binblock to read.</param>
    /// <returns>Returns anew byte array.</returns>
    public static byte[] DecodeBinBlock(this byte[] binBlock)
    {
        using var ms = new MemoryStream(binBlock);
        return ms.ReadBinBlock();
    }

    /// <summary>Encodes a byte array.</summary>
    /// <param name="data">Data to encode.</param>
    /// <param name="strip">Strip the padding. (Default = false)</param>
    /// <returns>Returns a base 64 encoded string.</returns>
    public static string EncodeBase64(this byte[] data, bool strip = false) => strip ? Convert.ToBase64String(data).TrimEnd('=') : Convert.ToBase64String(data);

    /// <summary>Encodes a byte array in binblock format.</summary>
    /// <param name="data">Byte array to encode.</param>
    /// <returns>Returns the binblock encoded data.</returns>
    public static byte[] EncodeBinBlock(this byte[] data)
    {
        using var ms = new MemoryStream();
        ms.WriteBinBlock(data);
        return ms.ToArray();
    }

    /// <summary>Calculates the number of bytes required to write the header of the binblock with the specified size.</summary>
    /// <param name="dataLength">Data length following the binblock header.</param>
    /// <returns>Returns the number of bytes required to write the header of the binblock.</returns>
    /// <exception cref="ArgumentOutOfRangeException">dataLength must be zero or greater!</exception>
    public static long GetBinBlockHeaderLength(long dataLength)
    {
        if (dataLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataLength), $"{nameof(dataLength)} must be zero or greater!");
        }
        var len = 1L;
        var dataLengthString = Encoding.ASCII.GetBytes($"{dataLength}");
        if (dataLengthString.Length > 9)
        {
            len += 2; // require parentheses
        }
        var prefix = Encoding.ASCII.GetBytes($"{dataLengthString.Length}");
        len += prefix.Length;
        len += dataLengthString.Length;
        return len;
    }

    /// <summary>Calculates the total number of bytes required to write the binblock</summary>
    /// <param name="dataLength">Data length following the binblock header.</param>
    /// <returns>Returns the total number of bytes required to write the binblock.</returns>
    public static long GetBinBlockLength(long dataLength)
    {
        var len = GetBinBlockHeaderLength(dataLength);
        len += dataLength;
        return len;
    }

    /// <summary>Gets the element type of an generic IList{TElement} or Array TElement[].</summary>
    /// <param name="content">Content to check</param>
    /// <returns>Returns the typeof(TElement) or throws an exception.</returns>
    /// <exception cref="InvalidOperationException">Could not determine array or list element type!</exception>
    public static Type GetElementType(object content)
    {
        try
        {
            var type = content.GetType();
            return !type.IsArray ? type.GetGenericArguments().Single() : type.GetElementType() ?? throw new NullReferenceException();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not determine array or list element type!", ex);
        }
    }

    /// <summary>Gets the <see cref="DxValueFormat"/> used for the specified dotnet type.</summary>
    /// <param name="type">Type to match.</param>
    /// <returns>Returns a <see cref="DxValueFormat"/> value for the specified type.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static DxValueFormat GetValueFormat(this Type type)
    {
        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            return args.Length == 1 && args[0].IsValueType
                ? GetValueFormat(args[0])
                : throw new InvalidOperationException($"Cannot determine {nameof(DxValueFormat)} of type {type}!");
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return true == elementType?.IsValueType
                ? GetValueFormat(elementType)
                : throw new InvalidOperationException($"Cannot determine {nameof(DxValueFormat)} of type {type}!");
        }

        return
            (type == typeof(byte)) ? DxValueFormat.UInt8 :
            (type == typeof(sbyte)) ? DxValueFormat.Int8 :
            (type == typeof(ushort)) ? DxValueFormat.UInt16 :
            (type == typeof(short)) ? DxValueFormat.Int16 :
            (type == typeof(uint)) ? DxValueFormat.UInt32 :
            (type == typeof(int)) ? DxValueFormat.Int32 :
            (type == typeof(ulong)) ? DxValueFormat.UInt64 :
            (type == typeof(long)) ? DxValueFormat.Int64 :
            (type == typeof(float)) ? DxValueFormat.Float32 :
            (type == typeof(double)) ? DxValueFormat.Float64 :
            throw new InvalidOperationException($"Cannot determine {nameof(DxValueFormat)} of type {type}!");
    }

    /// <summary>Checks whether a filename contain invalid characters or not.</summary>
    /// <param name="fileName">Filename to check</param>
    /// <returns>Returns true if the filename is valid.</returns>
    public static bool IsValidFileName(string fileName) => fileName.IndexOfAny(InvalidCharacters) == -1;

    /// <summary>Loads all values of a <see cref="DxBinaryValues"/> instance.</summary>
    /// <typeparam name="TValue">Expected return type.</typeparam>
    /// <param name="settings">Settings used to find and read files.</param>
    /// <param name="binaryValues">Instance to load from.</param>
    /// <returns>Returns a new list of values.</returns>
    /// <exception cref="NotSupportedException">XChangeSourceFormat.{SourceFormat} is not supported at XChangeBinaryValues.Source!</exception>
    /// <exception cref="InvalidOperationException">Cannot convert values from {SourceFormat} to {TargetFormat}!</exception>
    public static IList<TValue>? LoadValues<TValue>(this DxSettings settings, DxBinaryValues? binaryValues) where TValue : struct
    {
        if (binaryValues?.Content is not null)
        {
            return LoadBinaryValuesContent<TValue>(binaryValues);
        }

        if (binaryValues?.Source is null)
        {
            return null;
        }

        IList<TValue>? values;
        switch (binaryValues.SourceFormat)
        {
            default:
            {
                throw new NotSupportedException($"{nameof(binaryValues.SourceFormat)} {binaryValues.SourceFormat} is not supported!");
            }
            case DxBinaryFormat.Raw:
            {
                if (binaryValues.Source is null) throw new InvalidOperationException($"{nameof(binaryValues.SourceFormat)} {binaryValues.SourceFormat} requires a set {nameof(binaryValues.Source)} property!");
                var block = settings.ReadSource(binaryValues.Source);
                values = ConvertBinaryValues<TValue>(binaryValues, block);
                break;
            }
            case DxBinaryFormat.BinBlock:
            {
                if (binaryValues.Source is null) throw new InvalidOperationException($"{nameof(binaryValues.SourceFormat)} {binaryValues.SourceFormat} requires a set {nameof(binaryValues.Source)} or {nameof(binaryValues.Content)} property!");
                var block = settings.ReadSource(binaryValues.Source);
                block = DecodeBinBlock(block);
                values = ConvertBinaryValues<TValue>(binaryValues, block);
                break;
            }
            case DxBinaryFormat.Text:
            {
                if (binaryValues.Source is null) throw new InvalidOperationException($"{nameof(binaryValues.SourceFormat)} {binaryValues.SourceFormat} requires a set {nameof(binaryValues.Source)} or {nameof(binaryValues.Content)} property!");
                var content = Encoding.UTF8.GetString(settings.ReadSource(binaryValues.Source));
                //remove BOM if present
                if (content.StartsWith("\uFEFF", StringComparison.Ordinal) || content.StartsWith("\uFFFE", StringComparison.Ordinal))
                {
                    content = content.Substring(1);
                }
                values = DxExtensions.DecodeTextValues<TValue>(content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
                break;
            }
        }
        if (binaryValues.ValueCount == 0)
        {
            Trace.TraceWarning($"{nameof(binaryValues.ValueCount)} is unset at binary values {binaryValues.Source}!");
        }
        else if (values.Count != binaryValues.ValueCount)
        {
            throw new InvalidDataException($"Decoded value count {values.Count} does not match expected value count of {binaryValues.ValueCount}!");
        }
        return values;
    }

    /// <summary>Provides reading of a binblock.</summary>
    /// <param name="source">Stream to read.</param>
    /// <returns>Returns a new byte array.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="EndOfStreamException"></exception>
    public static byte[] ReadBinBlock(this Stream source)
    {
        //read start tag
        var value = source.ReadByte();
        if (value != '#') throw new InvalidOperationException("Start marker not present!");

        //read length of lenght
        value = source.ReadByte();
        if (value < 0) throw new EndOfStreamException();

        int numberOfLengthBytes;
        //is extended length (>9 bytes)
        if (value == '(')
        {
            //yes, read until closing bracket
            var length = new List<byte>();
            for (; ; )
            {
                value = source.ReadByte();
                if (value < 0) throw new EndOfStreamException();
                if (value == ')') break;
                length.Add((byte)value);
            }
            //real length of length read
            if (length.Count < 1) throw new InvalidOperationException("Length could not be read!");
            try { numberOfLengthBytes = int.Parse(Encoding.ASCII.GetString([.. length])); }
            catch (Exception ex) { throw new InvalidOperationException("Length could not be read!", ex); }
        }
        else
        {
            numberOfLengthBytes = value - (byte)'0';
            if (numberOfLengthBytes < 0) throw new EndOfStreamException();
        }

        var dataLengthBytes = new byte[numberOfLengthBytes];
        value = source.Read(dataLengthBytes, 0, numberOfLengthBytes);
        if (value < numberOfLengthBytes) throw new EndOfStreamException();

        // determine the exact size of the data. It is used to check wheter the user provided length value was correct.
        var currentPos = source.Position;
        var memoryStream = new MemoryStream();
        source.CopyTo(memoryStream);
        var actualDataLength = memoryStream.Seek(0, SeekOrigin.End);
        source.Position = currentPos;

        long dataLength;
        try { dataLength = long.Parse(Encoding.ASCII.GetString(dataLengthBytes)); }
        catch (Exception ex) { throw new InvalidOperationException("Length could not be read!", ex); }
        // check that the user provided length does not exceed the actual length. Otherwise blocks of memory are allocated that are never used
        if (actualDataLength != dataLength) throw new InvalidOperationException($"The actual length of {actualDataLength} doesn't match the provided length of {dataLength}");

        var result = new byte[dataLength];
        var copy = result.Length;
        while (copy > 0)
        {
            value = source.Read(result, 0, result.Length);
            if (value < 1) throw new EndOfStreamException();
            copy -= value;
        }
        return result;
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a byte</exception>
    public static void Scale(this byte[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(byte.MinValue, Math.Min(byte.MaxValue, (byte)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a ushort</exception>
    public static void Scale(this ushort[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, (ushort)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a uint</exception>
    public static void Scale(this uint[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(uint.MinValue, Math.Min(uint.MaxValue, (uint)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a ulong</exception>
    public static void Scale(this ulong[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(ulong.MinValue, Math.Min(ulong.MaxValue, (ulong)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a sbyte</exception>
    public static void Scale(this sbyte[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, (sbyte)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a short</exception>
    public static void Scale(this short[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(short.MinValue, Math.Min(short.MaxValue, (short)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a int</exception>
    public static void Scale(this int[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(int.MinValue, Math.Min(int.MaxValue, (int)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    /// <exception cref="OverflowException">Thrown if the result of the calculation exceeds the range of a long</exception>
    public static void Scale(this long[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = Math.Max(long.MinValue, Math.Min(long.MaxValue, (long)((array[i] * scale) + offset)));
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    public static void Scale(this double[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = ((array[i] * scale) + offset);
            }
        }
    }

    /// <summary>Perform a scaling operation on all values.</summary>
    /// <param name="array">Array to recalculate in place</param>
    /// <param name="scale">scaling value</param>
    /// <param name="offset">offset to add after scaling</param>
    public static void Scale(this float[] array, double scale, double offset)
    {
        if (scale != 1 && offset != 0)
        {
            for (var i = 0; i < array.LongLength; i++)
            {
                array[i] = (float)((array[i] * scale) + offset);
            }
        }
    }

    /// <summary>Writes a byte array binblock encoded to the stream.</summary>
    /// <param name="target">Stream to write to.</param>
    /// <param name="data">Byte array to write.</param>
    public static void WriteBinBlock(this Stream target, byte[] data)
    {
        WriteBinBlockHeader(target, data.Length);
        target.Write(data, 0, data.Length);
    }

    /// <summary>Writes a byte array binblock encoded to the stream.</summary>
    /// <param name="target">Stream to write to.</param>
    /// <param name="dataLength">Byte count to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">dataLength must be zero or greater!</exception>
    public static void WriteBinBlockHeader(this Stream target, long dataLength)
    {
        if (dataLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataLength), $"{nameof(dataLength)} must be zero or greater!");
        }
        //write marker
        target.WriteByte((byte)'#');
        //write length of length
        var dataLengthString = Encoding.ASCII.GetBytes($"{dataLength}");
        if (dataLengthString.Length > 9)
        {
            target.WriteByte((byte)'(');
        }
        var prefix = Encoding.ASCII.GetBytes($"{dataLengthString.Length}");
        target.Write(prefix, 0, prefix.Length);
        if (dataLengthString.Length > 9)
        {
            target.WriteByte((byte)')');
        }
        target.Write(dataLengthString, 0, dataLengthString.Length);
    }

    #endregion Public Methods
}
