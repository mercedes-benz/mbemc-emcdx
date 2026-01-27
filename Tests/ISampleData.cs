// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

interface ISampleData : IDxName, IDxDescription
{
    #region Public Properties

    byte ByteValue { get; }
    double DoubleValue { get; }
    IEnumerable<double> DoubleValues { get; }
    float FloatValue { get; }
    IEnumerable<float> FloatValues { get; }
    IEnumerable<short> Int16Values { get; }
    IEnumerable<int> Int32Values { get; }
    IEnumerable<long> Int64Values { get; }
    IEnumerable<sbyte> Int8Values { get; }
    int IntValue { get; }
    long LongValue { get; }
    sbyte SByteValue { get; }
    short ShortValue { get; }
    IEnumerable<ushort> UInt16Values { get; }
    IEnumerable<uint> UInt32Values { get; }
    IEnumerable<ulong> UInt64Values { get; }
    IEnumerable<byte> UInt8Values { get; }
    uint UIntValue { get; }
    ulong ULongValue { get; }
    ushort UShortValue { get; }

    #endregion Public Properties
}
