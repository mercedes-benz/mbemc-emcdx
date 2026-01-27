// SPDX-License-Identifier: MIT
using System.Text;
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestDxExtensions
{
    #region Public Methods

    [Fact]
    public void GetBinBlockHeaderLengthTestShouldThrow() => Assert.Throws<ArgumentOutOfRangeException>(() => DxExtensions.GetBinBlockHeaderLength(-1));

    [Theory]
    [InlineData(DxLoopOrder.Lex, new byte[] { }, new byte[] { })]
    [InlineData(DxLoopOrder.Colex, new byte[] { }, new byte[] { })]
    [InlineData(DxLoopOrder.Lex, new byte[] { byte.MinValue, 0, byte.MaxValue }, new byte[] { byte.MinValue, 0, byte.MaxValue })]
    [InlineData(DxLoopOrder.Colex, new byte[] { byte.MinValue, 0, byte.MaxValue }, new byte[] { byte.MinValue, 0, byte.MaxValue })]
    public void TestConvertBinaryValuesByte(DxLoopOrder loopOrder, byte[] data, byte[] expectedResult)
    {
        var differentByteOrder = DxExtensions.MachineType == DxEndian.LittleEndian ? DxEndian.BigEndian : DxEndian.LittleEndian;

        var binaryValues = new DxBinaryValues
        {
            ByteOrder = differentByteOrder,
            Format = DxValueFormat.UInt8,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<byte>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new double[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new double[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new double[] { double.NaN })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new double[] { double.NaN })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new double[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new double[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new double[] { -5.621885836375608E+274, 1.40159977307889E-309 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new double[] { -5.621885836375608E+274, 1.40159977307889E-309 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new double[] { double.NaN })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new double[] { double.NaN })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new double[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new double[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new double[] { double.NaN, 7.949928895127363E-275 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new double[] { double.NaN, 7.949928895127363E-275 })]
    public void TestConvertBinaryValuesDouble(DxLoopOrder loopOrder, DxEndian endian, byte[] data, double[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.Float64,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<double>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new float[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new float[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new float[] { float.NaN })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new float[] { float.NaN })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new float[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new float[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new float[] { -1.05505843E+37F, 9.2557E-41F })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new float[] { -1.05505843E+37F, 9.2557E-41F })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new float[] { float.NaN })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new float[] { float.NaN })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new float[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new float[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new float[] { float.NaN, 3.8204714E-37F })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new float[] { float.NaN, 3.8204714E-37F })]
    public void TestConvertBinaryValuesFloat(DxLoopOrder loopOrder, DxEndian endian, byte[] data, float[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.Float32,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<float>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new int[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new int[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new int[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new int[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new int[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new int[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new int[] { -50462977, 66051 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new int[] { -50462977, 66051 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new int[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new int[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new int[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new int[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new int[] { -66052, 50462976 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new int[] { -66052, 50462976 })]
    public void TestConvertBinaryValuesInt(DxLoopOrder loopOrder, DxEndian endian, byte[] data, int[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.Int32,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<int>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new long[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new long[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new long[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new long[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new long[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new long[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new long[] { -506097522914230529, 283686952306183 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new long[] { -506097522914230529, 283686952306183 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new long[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new long[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new long[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new long[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new long[] { -283686952306184, 506097522914230528 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new long[] { -283686952306184, 506097522914230528 })]
    public void TestConvertBinaryValuesLong(DxLoopOrder loopOrder, DxEndian endian, byte[] data, long[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.Int64,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<long>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, new byte[] { }, new sbyte[] { })]
    [InlineData(DxLoopOrder.Colex, new byte[] { }, new sbyte[] { })]
    [InlineData(DxLoopOrder.Lex, new byte[] { byte.MinValue, 127, byte.MaxValue }, new sbyte[] { 0, 127, -1 })]
    [InlineData(DxLoopOrder.Colex, new byte[] { byte.MinValue, 127, byte.MaxValue }, new sbyte[] { 0, 127, -1 })]
    public void TestConvertBinaryValuesSbyte(DxLoopOrder loopOrder, byte[] data, sbyte[] expectedResult)
    {
        var differentByteOrder = DxExtensions.MachineType == DxEndian.LittleEndian ? DxEndian.BigEndian : DxEndian.LittleEndian;
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = differentByteOrder,
            Format = DxValueFormat.Int8,
            LoopOrder = loopOrder
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<sbyte>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new short[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new short[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new short[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new short[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue }, new short[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue }, new short[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 1, 0 }, new short[] { -257, 1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 1, 0 }, new short[] { -257, 1 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new short[] { -1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new short[] { -1 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue }, new short[] { 0 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue }, new short[] { 0 })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 1, 0 }, new short[] { -2, 256 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 1, 0 }, new short[] { -2, 256 })]
    public void TestConvertBinaryValuesShort(DxLoopOrder loopOrder, DxEndian endian, byte[] data, short[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.Int16,
            LoopOrder = loopOrder
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<short>(data));
    }

    [Fact]
    public void TestConvertBinaryValuesShouldThrow()
    {
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<sbyte>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<short>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<int>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<long>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<byte>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<ushort>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<uint>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<ulong>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Float64 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<float>([]));
        }
        {
            var binaryValues = new DxBinaryValues { Format = DxValueFormat.Int8 };
            Assert.Throws<InvalidOperationException>(() => binaryValues.ConvertBinaryValues<double>([]));
        }
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new uint[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new uint[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new uint[] { uint.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new uint[] { uint.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new uint[] { uint.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new uint[] { uint.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new uint[] { 4244504319, 66051 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new uint[] { 4244504319, 66051 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new uint[] { uint.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new uint[] { uint.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new uint[] { uint.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new uint[] { uint.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new uint[] { 4294901244, 50462976 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 3, 2, 1, 0 }, new uint[] { 4294901244, 50462976 })]
    public void TestConvertBinaryValuesUInt(DxLoopOrder loopOrder, DxEndian endian, byte[] data, uint[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.UInt32,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<uint>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new ulong[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new ulong[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new ulong[] { ulong.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new ulong[] { ulong.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new ulong[] { ulong.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new ulong[] { ulong.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new ulong[] { 17940646550795321087, 283686952306183 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new ulong[] { 17940646550795321087, 283686952306183 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new ulong[] { ulong.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, new ulong[] { ulong.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new ulong[] { ulong.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue }, new ulong[] { ulong.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new ulong[] { 18446460386757245432, 506097522914230528 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 253, 252, 251, 250, 249, 248, 7, 6, 5, 4, 3, 2, 1, 0 }, new ulong[] { 18446460386757245432, 506097522914230528 })]
    public void TestConvertBinaryValuesULong(DxLoopOrder loopOrder, DxEndian endian, byte[] data, ulong[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.UInt64,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<ulong>(data));
    }

    [Theory]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { }, new ushort[] { })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { }, new ushort[] { })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new ushort[] { ushort.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new ushort[] { ushort.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue }, new ushort[] { ushort.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { byte.MinValue, byte.MinValue }, new ushort[] { ushort.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.LittleEndian, new byte[] { 255, 254, 1, 0 }, new ushort[] { 65279, 1 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.LittleEndian, new byte[] { 255, 254, 1, 0 }, new ushort[] { 65279, 1 })]

    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new ushort[] { ushort.MaxValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MaxValue, byte.MaxValue }, new ushort[] { ushort.MaxValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue }, new ushort[] { ushort.MinValue })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { byte.MinValue, byte.MinValue }, new ushort[] { ushort.MinValue })]
    [InlineData(DxLoopOrder.Lex, DxEndian.BigEndian, new byte[] { 255, 254, 1, 0 }, new ushort[] { 65534, 256 })]
    [InlineData(DxLoopOrder.Colex, DxEndian.BigEndian, new byte[] { 255, 254, 1, 0 }, new ushort[] { 65534, 256 })]
    public void TestConvertBinaryValuesUShort(DxLoopOrder loopOrder, DxEndian endian, byte[] data, ushort[] expectedResult)
    {
        var binaryValues = new DxBinaryValues
        {
            ByteOrder = endian,
            Format = DxValueFormat.UInt16,
            LoopOrder = loopOrder,
        };
        Assert.Equal(expectedResult, binaryValues.ConvertBinaryValues<ushort>(data));
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 3)]
    [InlineData(123_456_789, 11)]
    [InlineData(1_234_567_890, 15)]
    [InlineData(long.MaxValue, 24)]
    public void TestGetBinBlockHeaderLength(long dataLength, int expectedResult) => Assert.Equal(expectedResult, DxExtensions.GetBinBlockHeaderLength(dataLength));

    [Fact]
    public void TestGetBinBlockLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Equal(3, DxExtensions.GetBinBlockLength(0));
            Assert.Equal(4, DxExtensions.GetBinBlockLength(1));
            Assert.Equal(15 + 1_234_567_890, DxExtensions.GetBinBlockLength(1_234_567_890));
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => DxExtensions.GetBinBlockHeaderLength(-1));
    }

    [Fact]
    public void TestGetElementType()
    {
        object invalidInput = new Dictionary<int, string>();
        Assert.Throws<InvalidOperationException>(() => DxExtensions.GetElementType(invalidInput));

        object validArray = new char[5];
        Assert.Equal(typeof(char), DxExtensions.GetElementType(validArray));

        object validIList = new List<int>();
        Assert.Equal(typeof(int), DxExtensions.GetElementType(validIList));
    }

    [Theory]
    [InlineData("fileName", "fileName")]
    [InlineData("", "")]
    [InlineData("\uD800\uDC00", "")]
    [InlineData("file Name", "file-Name")]
    [InlineData(" file Name ", "file-Name")]
    [InlineData("\uD800fileName", "fileName")]
    [InlineData("file\uA00A-_\uD800Name", "file-Name")]
    [InlineData("fileName\uA00A\uD800", "fileName")]
    public void TestGetValidFileName(string fileName, string expectedResult) => Assert.Equal(expectedResult, DxExtensions.CreateFileName(fileName));

    [Theory]
    [InlineData(typeof(byte[]), DxValueFormat.UInt8)]
    [InlineData(typeof(sbyte[]), DxValueFormat.Int8)]
    [InlineData(typeof(ushort[]), DxValueFormat.UInt16)]
    [InlineData(typeof(short[]), DxValueFormat.Int16)]
    [InlineData(typeof(uint[]), DxValueFormat.UInt32)]
    [InlineData(typeof(int[]), DxValueFormat.Int32)]
    [InlineData(typeof(ulong[]), DxValueFormat.UInt64)]
    [InlineData(typeof(long[]), DxValueFormat.Int64)]
    [InlineData(typeof(float[]), DxValueFormat.Float32)]
    [InlineData(typeof(double[]), DxValueFormat.Float64)]
    public void TestGetValueFormatArray(Type type, DxValueFormat expectedType) => Assert.Equal(expectedType, type.GetValueFormat());

    [Fact]
    public void TestGetValueFormatArrayShouldThrow() => Assert.Throws<InvalidOperationException>(() => typeof(string[]).GetValueFormat());

    [Theory]
    [InlineData(typeof(List<byte>), DxValueFormat.UInt8)]
    [InlineData(typeof(List<sbyte>), DxValueFormat.Int8)]
    [InlineData(typeof(List<ushort>), DxValueFormat.UInt16)]
    [InlineData(typeof(List<short>), DxValueFormat.Int16)]
    [InlineData(typeof(List<uint>), DxValueFormat.UInt32)]
    [InlineData(typeof(List<int>), DxValueFormat.Int32)]
    [InlineData(typeof(List<ulong>), DxValueFormat.UInt64)]
    [InlineData(typeof(List<long>), DxValueFormat.Int64)]
    [InlineData(typeof(List<float>), DxValueFormat.Float32)]
    [InlineData(typeof(List<double>), DxValueFormat.Float64)]
    public void TestGetValueFormatGeneric(Type type, DxValueFormat expectedType) => Assert.Equal(expectedType, type.GetValueFormat());

    [Fact]
    public void TestGetValueFormatGenericShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Dictionary<,>).GetValueFormat());
        Assert.Throws<InvalidOperationException>(() => typeof(List<string>).GetValueFormat());
    }

    [Theory]
    [InlineData("#(10)1000000000", 1_000_000_000)]
    public void TestReadBinBlock(string header, int dataLength)
    {
        var dummyData = new string('A', dataLength);
        var binBlockData = header + dummyData;
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(binBlockData));

        Assert.True(stream.ReadBinBlock().SequenceEqual((Encoding.ASCII.GetBytes(dummyData))));
    }

    [Theory]
    [InlineData("(", 0, typeof(InvalidOperationException))]
    [InlineData("#", 0, typeof(EndOfStreamException))]
    [InlineData("#(", 0, typeof(EndOfStreamException))]
    [InlineData("#()", 0, typeof(InvalidOperationException))]
    [InlineData("#(a)", 0, typeof(InvalidOperationException))]
    [InlineData("#0", 0, typeof(InvalidOperationException))]
    [InlineData("#1", 1, typeof(InvalidOperationException))]
    [InlineData("#(10)", 10, typeof(InvalidOperationException))]
    [InlineData("#15", 4, typeof(InvalidOperationException))]
    [InlineData("#(10)1000000001", 1_000_000_000, typeof(InvalidOperationException))]
    public void TestReadBinBlockShouldThrow(string header, int dataLength, Type expectedException)
    {
        var dummyData = new string('A', dataLength);
        var binBlockData = header + dummyData;
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(binBlockData));

        Assert.Throws(expectedException, () => stream.ReadBinBlock().SequenceEqual((Encoding.ASCII.GetBytes(dummyData))));
    }

    [Theory]
    [InlineData(new byte[] { 1, 2, 3 }, 2.0, 10.0, new byte[] { 12, 14, 16 })]
    [InlineData(new byte[] { }, 2.0, 10.0, new byte[] { })]
    [InlineData(new byte[] { 1, 2, 3 }, 1.0, 0.0, new byte[] { 1, 2, 3 })]
    [InlineData(new byte[] { 1, 2, 3 }, 1.1, 1.1, new byte[] { (byte)((1 * 1.1) + 1.1), (byte)((2 * 1.1) + 1.1), (byte)((3 * 1.1) + 1.1) })]
    public void TestScaleByte(byte[] array, double scale, double offset, byte[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new byte[] { byte.MinValue, byte.MinValue, byte.MinValue }, 12.34, -12.34)]
    [InlineData(new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue }, 12.34, 12.34)]
    public void TestScaleByteShouldThrow(byte[] data, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(data, scale, offset));

    [Theory]
    [InlineData(new double[] { 1.0, 2.0, 3.0 }, 2.0, 10.0, new double[] { 12.0, 14.0, 16.0 })]
    [InlineData(new double[] { }, 2.0, 10.0, new double[] { })]
    [InlineData(new double[] { 1, 2, 3 }, 1.0, 0.0, new double[] { 1, 2, 3 })]
    [InlineData(new double[] { 1, 2, 3 }, 1.1, 1.1, new double[] { ((1 * 1.1) + 1.1), ((2 * 1.1) + 1.1), ((3 * 1.1) + 1.1) })]
    [InlineData(new double[] { double.MinValue, double.MinValue, double.MinValue }, 12.34, -12.34, new double[] { double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity })]
    [InlineData(new double[] { double.MaxValue, double.MaxValue, double.MaxValue }, 12.34, 12.34, new double[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity })]
    public void TestScaleDouble(double[] array, double scale, double offset, double[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new float[] { 1, 2, 3 }, 2.0, 10.0, new float[] { 12, 14, 16 })]
    [InlineData(new float[] { }, 2.0, 10.0, new float[] { })]
    [InlineData(new float[] { 1, 2, 3 }, 1.0, 0.0, new float[] { 1, 2, 3 })]
    [InlineData(new float[] { 1, 2, 3 }, 1.1, 1.1, new float[] { (float)((1 * 1.1) + 1.1), (float)((2 * 1.1) + 1.1), (float)((3 * 1.1) + 1.1) })]
    [InlineData(new float[] { float.MinValue, float.MinValue, float.MinValue }, 12.34, -12.34, new float[] { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity })]
    [InlineData(new float[] { float.MaxValue, float.MaxValue, float.MaxValue }, 12.34, 12.34, new float[] { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity })]
    public void TestScaleFloat(float[] array, double scale, double offset, float[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3 }, 2.0, 10.0, new int[] { 12, 14, 16 })]
    [InlineData(new int[] { }, 2.0, 10.0, new int[] { })]
    [InlineData(new int[] { 1, 2, 3 }, 1.0, 0.0, new int[] { 1, 2, 3 })]
    [InlineData(new int[] { 1, 2, 3 }, 1.1, 1.1, new int[] { (int)((1 * 1.1) + 1.1), (int)((2 * 1.1) + 1.1), (int)((3 * 1.1) + 1.1) })]
    public void TestScaleInt(int[] array, double scale, double offset, int[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new int[] { int.MinValue, int.MinValue, int.MinValue }, 12.34, -12.34)]
    [InlineData(new int[] { int.MaxValue, int.MaxValue, int.MaxValue }, 12.34, 12.34)]
    public void TestScaleIntShouldThrow(int[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new long[] { 1, 2, 3 }, 2.0, 10.0, new long[] { 12, 14, 16 })]
    [InlineData(new long[] { }, 2.0, 10.0, new long[] { })]
    [InlineData(new long[] { 1, 2, 3 }, 1.0, 0.0, new long[] { 1, 2, 3 })]
    [InlineData(new long[] { 1, 2, 3 }, 1.1, 1.1, new long[] { (long)((1 * 1.1) + 1.1), (long)((2 * 1.1) + 1.1), (long)((3 * 1.1) + 1.1) })]
    public void TestScaleLong(long[] array, double scale, double offset, long[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new long[] { long.MinValue, long.MinValue, long.MinValue }, 12.34, -12.34)]
    [InlineData(new long[] { long.MaxValue, long.MaxValue, long.MaxValue }, 12.34, 12.34)]
    public void TestScaleLongShouldThrow(long[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new sbyte[] { 1, 2, 3 }, 2.0, 10.0, new sbyte[] { 12, 14, 16 })]
    [InlineData(new sbyte[] { }, 2.0, 10.0, new sbyte[] { })]
    [InlineData(new sbyte[] { 1, 2, 3 }, 1.0, 0.0, new sbyte[] { 1, 2, 3 })]
    [InlineData(new sbyte[] { 1, 2, 3 }, 1.1, 1.1, new sbyte[] { (sbyte)((1 * 1.1) + 1.1), (sbyte)((2 * 1.1) + 1.1), (sbyte)((3 * 1.1) + 1.1) })]
    public void TestScaleSByte(sbyte[] array, double scale, double offset, sbyte[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new sbyte[] { sbyte.MinValue, sbyte.MinValue, sbyte.MinValue }, 12.34, -12.34)]
    [InlineData(new sbyte[] { sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue }, 12.34, 12.34)]
    public void TestScaleSByteShouldThrow(sbyte[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new short[] { 1, 2, 3 }, 2.0, 10.0, new short[] { 12, 14, 16 })]
    [InlineData(new short[] { }, 2.0, 10.0, new short[] { })]
    [InlineData(new short[] { 1, 2, 3 }, 1.0, 0.0, new short[] { 1, 2, 3 })]
    [InlineData(new short[] { 1, 2, 3 }, 1.1, 1.1, new short[] { (short)((1 * 1.1) + 1.1), (short)((2 * 1.1) + 1.1), (short)((3 * 1.1) + 1.1) })]
    public void TestScaleShort(short[] array, double scale, double offset, short[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new short[] { short.MinValue, short.MinValue, short.MinValue }, 12.34, -12.34)]
    [InlineData(new short[] { short.MaxValue, short.MaxValue, short.MaxValue }, 12.34, 12.34)]
    public void TestScaleShortShouldThrow(short[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new uint[] { 1, 2, 3 }, 2.0, 10.0, new uint[] { 12, 14, 16 })]
    [InlineData(new uint[] { }, 2.0, 10.0, new uint[] { })]
    [InlineData(new uint[] { 1, 2, 3 }, 1.0, 0.0, new uint[] { 1, 2, 3 })]
    [InlineData(new uint[] { 1, 2, 3 }, 1.1, 1.1, new uint[] { (uint)((1 * 1.1) + 1.1), (uint)((2 * 1.1) + 1.1), (uint)((3 * 1.1) + 1.1) })]
    public void TestScaleUInt(uint[] array, double scale, double offset, uint[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new uint[] { uint.MinValue, uint.MinValue, uint.MinValue }, -12.34, -12.34)]
    [InlineData(new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }, 12.34, 12.34)]
    public void TestScaleUIntShouldThrow(uint[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new ulong[] { 1, 2, 3 }, 2.0, 10.0, new ulong[] { 12, 14, 16 })]
    [InlineData(new ulong[] { }, 2.0, 10.0, new ulong[] { })]
    [InlineData(new ulong[] { 1, 2, 3 }, 1.0, 0.0, new ulong[] { 1, 2, 3 })]
    [InlineData(new ulong[] { 1, 2, 3 }, 1.1, 1.1, new ulong[] { (ulong)((1 * 1.1) + 1.1), (ulong)((2 * 1.1) + 1.1), (ulong)((3 * 1.1) + 1.1) })]
    public void TestScaleULong(ulong[] array, double scale, double offset, ulong[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new ulong[] { ulong.MinValue, ulong.MinValue, ulong.MinValue }, -12.34, -12.34)]
    [InlineData(new ulong[] { ulong.MaxValue, ulong.MaxValue, ulong.MaxValue }, 12.34, 12.34)]
    public void TestScaleULongShouldThrow(ulong[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Theory]
    [InlineData(new ushort[] { 1, 2, 3 }, 2.0, 10.0, new ushort[] { 12, 14, 16 })]
    [InlineData(new ushort[] { }, 2.0, 10.0, new ushort[] { })]
    [InlineData(new ushort[] { 1, 2, 3 }, 1.0, 0.0, new ushort[] { 1, 2, 3 })]
    [InlineData(new ushort[] { 1, 2, 3 }, 1.1, 1.1, new ushort[] { (ushort)((1 * 1.1) + 1.1), (ushort)((2 * 1.1) + 1.1), (ushort)((3 * 1.1) + 1.1) })]
    public void TestScaleUShort(ushort[] array, double scale, double offset, ushort[] expectedResult)
    {
        array.Scale(scale, offset);
        Assert.Equal(expectedResult, array);
    }

    [Theory]
    [InlineData(new ushort[] { ushort.MinValue, ushort.MinValue, ushort.MinValue }, -12.34, -12.34)]
    [InlineData(new ushort[] { ushort.MaxValue, ushort.MaxValue, ushort.MaxValue }, 12.34, 12.34)]
    public void TestScaleUShortShouldThrow(ushort[] array, double scale, double offset) => Assert.Throws<OverflowException>(() => DxExtensions.Scale(array, scale, offset));

    [Fact]
    public void TestWriteBinBlockHeader()
    {
        using (var stream = new MemoryStream())
        {
            long dataLength = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.WriteBinBlockHeader(dataLength));
        }

        using (var stream = new MemoryStream())
        {
            long dataLength = 0;
            var expectedHeaderLength = DxExtensions.GetBinBlockHeaderLength(dataLength);
            stream.WriteBinBlockHeader(dataLength);
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[expectedHeaderLength];
            stream.Read(buffer, 0, (int)expectedHeaderLength);
            Assert.Equal("#10", Encoding.ASCII.GetString(buffer));
        }

        using (var stream = new MemoryStream())
        {
            long dataLength = 1;
            var expectedHeaderLength = DxExtensions.GetBinBlockHeaderLength(dataLength);
            stream.WriteBinBlockHeader(dataLength);
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[expectedHeaderLength];
            stream.Read(buffer, 0, (int)expectedHeaderLength);
            Assert.Equal("#11", Encoding.ASCII.GetString(buffer));
        }

        using (var stream = new MemoryStream())
        {
            long dataLength = 1_000_000_000;
            var expectedHeaderLength = DxExtensions.GetBinBlockHeaderLength(dataLength);
            stream.WriteBinBlockHeader(dataLength);
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[expectedHeaderLength];
            stream.Read(buffer, 0, (int)expectedHeaderLength);
            Assert.Equal("#(10)1000000000", Encoding.ASCII.GetString(buffer));
        }
    }

    #endregion Public Methods
}
