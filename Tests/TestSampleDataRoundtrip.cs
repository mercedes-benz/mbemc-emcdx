// SPDX-License-Identifier: MIT
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestSampleDataRoundtrip : TestWithClearWorkingFolder
{
    #region Private Methods

    static SampleData GetSampleData() => new()
    {
        Name = "Sample data",
        Description = "Some sample data description.",
        DoubleValues = [double.MinValue, double.MaxValue, 1.2d, 13.371337e42d, 1.3371337e-42d],
        FloatValues = [float.MinValue, float.MaxValue, 1.2f, 13.37e37f, 1.337e-45f],
        Int8Values = [sbyte.MinValue, sbyte.MaxValue, 1, -64, 127],
        Int16Values = [short.MinValue, short.MaxValue, -7331, 1, -32768, 1337],
        Int32Values = [int.MinValue, int.MaxValue, 0x13371337, 0x1A2B3C4D, -0x1A2B3C4D, 305419896, -123456789],
        Int64Values = [long.MinValue, long.MaxValue, 0x1337133713371337, 0x1A2B3C4D5E6F7890, -0x1A2B3C4D5E6F7890],
        UInt8Values = [byte.MinValue, byte.MaxValue, 1, 64, 255],
        UInt16Values = [ushort.MinValue, ushort.MaxValue, 7331, 1, 32768, 1337,],
        UInt32Values = [uint.MinValue, uint.MaxValue, 0x13371337, 0x1A2B3C4D, 0xFDB97531, 305419896, 123456789],
        UInt64Values = [ulong.MinValue, ulong.MaxValue, 0x1337133713371337, 0x13371337, 0xFEDCBA9876543210, 0x0123456789ABCDEF],
        FloatValue = 13.37e7f,
        DoubleValue = 1337.7331e-19d,
        ByteValue = 254,
        SByteValue = -123,
        ShortValue = -31337,
        UShortValue = 42042,
        IntValue = -2013371337,
        UIntValue = 4242424242,
        LongValue = -8133713371337133742L,
        ULongValue = 0xFEEDFEEDFEEDFEEDUL,
    };

    static SampleData GetSampleDataForPlacementArray() => new()
    {
        Name = "Sample data",
        Description = "This dataset does not contain long and ulong values, because they do not work with placement array",
        DoubleValues = [double.MinValue, double.MaxValue, 1.2d, 13.371337e42d, 1.3371337e-42d],
        FloatValues = [float.MinValue, float.MaxValue, 1.2f, 13.37e37f, 1.337e-45f],
        Int8Values = [sbyte.MinValue, sbyte.MaxValue, 1, -64, 127],
        Int16Values = [short.MinValue, short.MaxValue, -7331, 1, -32768, 1337],
        Int32Values = [int.MinValue, int.MaxValue, 0x13371337, 0x1A2B3C4D, -0x1A2B3C4D, 305419896, -123456789],
        UInt8Values = [byte.MinValue, byte.MaxValue, 1, 64, 255],
        UInt16Values = [ushort.MinValue, ushort.MaxValue, 7331, 1, 32768, 1337],
        UInt32Values = [uint.MinValue, uint.MaxValue, 0x13371337, 0x1A2B3C4D, 0xFDB97531, 305419896, 123456789],
        FloatValue = 13.37e7f,
        DoubleValue = 1337.7331e-19d,
        ByteValue = 254,
        SByteValue = -123,
        ShortValue = -31337,
        UShortValue = 42042,
        IntValue = -2013371337,
        UIntValue = 4242424242,
    };

    static SampleDataWithArrays GetSampleDataWithArrays() => new()
    {
        Name = "Sample data",
        Description = "Some sample data description.",
        DoubleValues = [double.MinValue, double.MaxValue, 1.2d, 13.371337e42d, 1.3371337e-42d],
        FloatValues = [float.MinValue, float.MaxValue, 1.2f, 13.37e37f, 1.337e-45f],
        Int8Values = [sbyte.MinValue, sbyte.MaxValue, 1, -64, 127],
        Int16Values = [short.MinValue, short.MaxValue, -7331, 1, -32768, 1337,],
        Int32Values = [int.MinValue, int.MaxValue, 0x13371337, 0x1A2B3C4D, -0x1A2B3C4D, 305419896, -123456789],
        Int64Values = [long.MinValue, long.MaxValue, 0x1337133713371337, 0x1A2B3C4D5E6F7890, -0x1A2B3C4D5E6F7890],
        UInt8Values = [byte.MinValue, byte.MaxValue, 1, 64, 255],
        UInt16Values = [ushort.MinValue, ushort.MaxValue, 7331, 1, 32768, 1337],
        UInt32Values = [uint.MinValue, uint.MaxValue, 0x13371337, 0x1A2B3C4D, 0xFDB97531, 305419896, 123456789],
        UInt64Values = [ulong.MinValue, ulong.MaxValue, 0x1337133713371337, 0x13371337, 0xFEDCBA9876543210, 0x0123456789ABCDEF],
        FloatValue = 13.37e7f,
        DoubleValue = 1337.7331e-19d,
        ByteValue = 254,
        SByteValue = -123,
        ShortValue = -31337,
        UShortValue = 42042,
        IntValue = -2013371337,
        UIntValue = 4242424242,
        LongValue = -8133713371337133742L,
        ULongValue = 0xFEEDFEEDFEEDFEEDUL,
    };

    static SampleDataWithArrays GetSampleDataWithArraysForPlacementArray() => new()
    {
        Name = "Sample data",
        Description = "This dataset does not contain long and ulong values, because they do not work with placement array",
        DoubleValues = [double.MinValue, double.MaxValue, 1.2d, 13.371337e42d, 1.3371337e-42d],
        FloatValues = [float.MinValue, float.MaxValue, 1.2f, 13.37e37f, 1.337e-45f],
        Int8Values = [sbyte.MinValue, sbyte.MaxValue, 1, -64, 127],
        Int16Values = [short.MinValue, short.MaxValue, -7331, 1, -32768, 1337],
        Int32Values = [int.MinValue, int.MaxValue, 0x13371337, 0x1A2B3C4D, -0x1A2B3C4D, 305419896, -123456789],
        UInt8Values = [byte.MinValue, byte.MaxValue, 1, 64, 255],
        UInt16Values = [ushort.MinValue, ushort.MaxValue, 7331, 1, 32768, 1337],
        UInt32Values = [uint.MinValue, uint.MaxValue, 0x13371337, 0x1A2B3C4D, 0xFDB97531, 305419896, 123456789],
        FloatValue = 13.37e7f,
        DoubleValue = 1337.7331e-19d,
        ByteValue = 254,
        SByteValue = -123,
        ShortValue = -31337,
        UShortValue = 42042,
        IntValue = -2013371337,
        UIntValue = 4242424242,
    };

    static void TestEquals(ISampleData expected, ISampleData item)
    {
        Assert.Multiple(() =>
        {
            Assert.Equal(item.Name, expected.Name);
            Assert.Equal(item.Description, expected.Description);
            Assert.Equal(item.DoubleValues, expected.DoubleValues);
            Assert.Equal(item.FloatValues, expected.FloatValues);
            Assert.Equal(item.Int8Values, expected.Int8Values);
            Assert.Equal(item.Int16Values, expected.Int16Values);
            Assert.Equal(item.Int32Values, expected.Int32Values);
            Assert.Equal(item.Int64Values, expected.Int64Values);
            Assert.Equal(item.UInt8Values, expected.UInt8Values);
            Assert.Equal(item.UInt16Values, expected.UInt16Values);
            Assert.Equal(item.UInt32Values, expected.UInt32Values);
            Assert.Equal(item.UInt64Values, expected.UInt64Values);
            Assert.Equal(item.SByteValue, expected.SByteValue);
            Assert.Equal(item.ByteValue, expected.ByteValue);
            Assert.Equal(item.ShortValue, expected.ShortValue);
            Assert.Equal(item.UShortValue, expected.UShortValue);
            Assert.Equal(item.IntValue, expected.IntValue);
            Assert.Equal(item.UIntValue, expected.UIntValue);
            Assert.Equal(item.LongValue, expected.LongValue);
            Assert.Equal(item.ULongValue, expected.ULongValue);
        });
    }

    static void TestPlacement(DxBinaryFormat expected, IEnumerable<DxItemWithValues> enumerable)
    {
        foreach (var variable in enumerable)
        {
            if (variable.BinaryValues is not null)
            {
                Assert.Equal(expected, variable.BinaryValues.SourceFormat);
            }
            else
            {
                Assert.Equal(1, variable.Values?.Count);
            }
        }
    }

    #endregion Private Methods

    #region Public Methods



    [Theory]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Undefined)] //internally it is converted to DxBinaryFormat.Raw
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock)]
    public void TestBinFileSizeRestrictionShouldThrow(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleData() with { UInt8Values = new byte[(1 << 15) + 1] };
        var writer = new DxWriter(TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0, MaxBinFileSize = 1 << 15 });
        writer.Write(sample);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Save());
    }


    [Theory]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Raw, typeof(NotSupportedException))]
    public void TestPlacementContentWithFormatRawShouldThrow(DxDataPlacement placement, DxBinaryFormat format, Type expectedException)
    {
        {
            var sample = GetSampleData();

            var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
            writer.Write(sample);
            Assert.Throws(expectedException, () => writer.Save());
        }
        {
            var sample = GetSampleDataWithArrays();

            var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
            writer.Write(sample);
            Assert.Throws(expectedException, () => writer.Save());
        }
    }

    [Theory]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Text, typeof(InvalidOperationException))]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.BinBlock, typeof(InvalidOperationException))]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Raw, typeof(InvalidOperationException))]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Base64, typeof(InvalidOperationException))]
    /// <summary>This should throw, because (u)longs are not convertible to floats without precision loss</summary>
    public void TestReadShouldThrow(DxDataPlacement placement, DxBinaryFormat format, Type expectedException)
    {
        var sample = GetSampleData();

        var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
        writer.Write(sample);
        writer.Save();

        var reader = DxReader.ReadFile(writer.FileNames.First(), TestSettings);

        Assert.Throws(expectedException, () => reader.Read<SampleData>().ToList());
        Assert.Throws(expectedException, () => reader.Read<SampleDataWithArrays>().ToList());
    }

    [Theory]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Base64)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw)]
    public void TestReadWriteWithoutMapping2(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleDataWithArrays();

        var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
        writer.Write(sample);
        writer.Save();

        {
            var reader = DxReader.ReadFile(writer.FileNames.First(), TestSettings);
            var content = reader.Read<SampleData>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }

        {
            var reader = DxReader.ReadFile(writer.FileNames.First(), TestSettings);
            var content = reader.Read<SampleDataWithArrays>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }
    }

    [Theory]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Base64)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw)]
    public void TestRoundtripWithoutMapping(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample1 = GetSampleData() with { Name = "Sample1" };
        var sample2 = GetSampleData() with { Name = "Sample2" };

        var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
        writer.Write([sample1, sample2]);
        writer.Save();

        {
            var reader = DxReader.ReadFile(writer.FileNames.First(), TestSettings);
            TestPlacement(format, reader.File.Data.SelectMany(d => d.Variables));

            var content = reader.Read<SampleData>().ToList();
            Assert.Equal(2, content.Count);

            //first
            {
                var item = content.First();
                TestEquals(sample1, item);
            }

            //second
            {
                var item = content.Skip(1).Single();
                TestEquals(sample2, item);
            }
        }

        {
            var reader = DxReader.ReadFile(writer.FileNames.First(), TestSettings);
            var content = reader.Read<SampleDataWithArrays>().ToList();
            Assert.Equal(2, content.Count);

            //first
            {
                var item = content.First();
                TestEquals(sample1, item);
            }

            //second
            {
                var item = content.Skip(1).Single();
                TestEquals(sample2, item);
            }
        }
    }

    [Theory]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Raw)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Base64)]
    public void TestRoundtripWithoutMappingPlacementArray(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleDataForPlacementArray();

        var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
        writer.Write(sample);
        writer.Save();
        Assert.Single(writer.FileNames);
        {
            var reader = DxReader.ReadFile(writer.FileNames.Single(), TestSettings);
            var content = reader.Read<SampleData>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }

        {
            var reader = DxReader.ReadFile(writer.FileNames.Single(), TestSettings);
            var content = reader.Read<SampleDataWithArrays>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }
    }

    [Theory]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Raw)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Base64)]
    public void TestRoundtripWithoutMappingPlacementArray2(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleDataWithArraysForPlacementArray();

        var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
        writer.Write(sample);
        writer.Save();
        Assert.Single(writer.FileNames);
        {
            var reader = DxReader.ReadFile(writer.FileNames.Single(), TestSettings);
            var content = reader.Read<SampleData>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }

        {
            var reader = DxReader.ReadFile(writer.FileNames.Single(), TestSettings);
            var content = reader.Read<SampleDataWithArrays>().ToList();
            Assert.Single(content);

            var item = content.Single();
            TestEquals(sample, item);
        }
    }

    [Theory]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Base64, typeof(NotImplementedException))]
    public void TestUnimplemented(DxDataPlacement placement, DxBinaryFormat format, Type expectedException)
    {
        {
            var sample = GetSampleData();

            var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
            writer.Write(sample);

            Assert.Throws(expectedException, () => writer.Save());
        }
        {
            var sample = GetSampleDataWithArrays();

            var writer = new DxWriter(settings: TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0 });
            writer.Write(sample);

            Assert.Throws(expectedException, () => writer.Save());
        }
    }

    #endregion Public Methods
}
