// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

record SampleData : ISampleData
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    [DxVariable]
    public IList<float> FloatValues { get; init; } = [];

    [DxVariable]
    public IList<double> DoubleValues { get; init; } = [];

    [DxVariable]
    public IList<sbyte> Int8Values { get; init; } = [];

    [DxVariable]
    public IList<short> Int16Values { get; init; } = [];

    [DxVariable]
    public IList<int> Int32Values { get; init; } = [];

    [DxVariable]
    public IList<long> Int64Values { get; init; } = [];

    [DxVariable]
    public IList<byte> UInt8Values { get; init; } = [];

    [DxVariable]
    public IList<ushort> UInt16Values { get; init; } = [];

    [DxVariable]
    public IList<uint> UInt32Values { get; init; } = [];

    [DxVariable]
    public IList<ulong> UInt64Values { get; init; } = [];

    [DxVariable]
    public float FloatValue { get; init; }

    [DxVariable]
    public double DoubleValue { get; init; }

    [DxVariable]
    public byte ByteValue { get; init; }

    [DxVariable]
    public sbyte SByteValue { get; init; }

    [DxVariable]
    public short ShortValue { get; init; }

    [DxVariable]
    public ushort UShortValue { get; init; }

    [DxVariable]
    public int IntValue { get; init; }

    [DxVariable]
    public uint UIntValue { get; init; }

    [DxVariable]
    public long LongValue { get; init; }

    [DxVariable]
    public ulong ULongValue { get; init; }

    IEnumerable<double> ISampleData.DoubleValues => DoubleValues;
    IEnumerable<float> ISampleData.FloatValues => FloatValues;
    IEnumerable<short> ISampleData.Int16Values => Int16Values;
    IEnumerable<int> ISampleData.Int32Values => Int32Values;
    IEnumerable<long> ISampleData.Int64Values => Int64Values;
    IEnumerable<sbyte> ISampleData.Int8Values => Int8Values;
    IEnumerable<ushort> ISampleData.UInt16Values => UInt16Values;
    IEnumerable<uint> ISampleData.UInt32Values => UInt32Values;
    IEnumerable<ulong> ISampleData.UInt64Values => UInt64Values;
    IEnumerable<byte> ISampleData.UInt8Values => UInt8Values;
}
