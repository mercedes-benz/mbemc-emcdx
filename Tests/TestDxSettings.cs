using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mbemc.DataExchange.Tests;
public class TestDxSettings : TestWithClearWorkingFolder
{
    static SampleData GetSampleData(int numBytes)
    {
        return new SampleData
        {
            UInt8Values = [.. Enumerable.Repeat(byte.MaxValue, numBytes)]
        };
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MaxValue)]
    [InlineData((1L << 31) + 1)]
    public void TestInvalidBinFileSizeRestrictionShouldThrow(long fileSizeLimit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DxSettings { MaxBinFileSize = fileSizeLimit });
        Assert.Throws<ArgumentOutOfRangeException>(() => new DxSettings { MaxEmcdxFileSize = fileSizeLimit });
    }

    [Theory]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Undefined)] //internally it is converted to DxBinaryFormat.Raw
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock)]
    public void TestBinFileSizeRestriction(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleData(1 << 20);
        var writer = new DxWriter(TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0, MaxBinFileSize = 1L << 31 });
        writer.Write(sample);
        writer.Save();
        //if it runs without exception, this test is passed.
    }

    [Theory]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw, 0)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text, 0)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Undefined, 0)] //internally it is converted to DxBinaryFormat.Raw
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock, 0)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Raw, 1 << 10)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Text, 1 << 10)]
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.Undefined, 1 << 10)] //internally it is converted to DxBinaryFormat.Raw
    [InlineData(DxDataPlacement.BinaryFile, DxBinaryFormat.BinBlock, 1 << 10)]
    public void TestBinFileSizeRestrictionShouldThrow(DxDataPlacement placement, DxBinaryFormat format, int fileSizeLimit)
    {
        var sample = GetSampleData(fileSizeLimit + 1);
        var writer = new DxWriter(TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0, MaxBinFileSize = fileSizeLimit });
        writer.Write(sample);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Save());
    }

    [Theory]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Raw)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Undefined)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Base64)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.BinBlock)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Text)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Base64)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.BinBlock)]
    public void TestJsonFileSizeRestriction(DxDataPlacement placement, DxBinaryFormat format)
    {
        var sample = GetSampleData(1 << 20);
        var writer = new DxWriter(TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0, MaxEmcdxFileSize = 1L << 31 });
        writer.Write(sample);
        writer.Save();
        //if it runs without exception, this test is passed.
    }

    [Theory]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Raw, 1 << 20)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Text, 1 << 20)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Undefined, 1 << 20)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.Base64, 1 << 20)]
    [InlineData(DxDataPlacement.Array, DxBinaryFormat.BinBlock, 1 << 20)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Text, 1 << 20)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.Base64, 1 << 20)]
    [InlineData(DxDataPlacement.Content, DxBinaryFormat.BinBlock, 1 << 20)]
    public void TestJsonFileSizeRestrictionShouldThrow(DxDataPlacement placement, DxBinaryFormat format, int fileSizeLimit)
    {
        // estimated sample size in bytes is larger than settings.MaxEmcdxFileSize
        var sample = GetSampleData(fileSizeLimit + 1);
        var writer = new DxWriter(TestSettings with { Placement = placement, BinaryFormat = format, ValueCountLimit = 0, MaxEmcdxFileSize = fileSizeLimit });
        writer.Write(sample);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Save());
    }
}
