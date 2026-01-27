// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides inlined or external binary data.</summary>
public record class DxBinaryValues : IEquatable<DxBinaryValues>
{
    /// <summary>Gets the endianess of the binary stream.</summary>
    public DxEndian ByteOrder { get; init; }

    /// <summary>Gets the (base64) content string containing the data.</summary>
    /// <remarks>This is exclusive with <see cref="Source"/>.</remarks>
    public string? Content { get; init; }

    /// <summary>Gets the dimensions of the mapping from variables to the value array.</summary>
    /// <remarks>This array matches the size of <see cref="DxMapping.VariableNames"/> and contains the dimension for each variable.</remarks>
    public IList<int> Dimensions { get; init; } = [];

    /// <summary>Gets the value format at the binary stream.</summary>
    public DxValueFormat Format { get; init; }

    /// <summary>Gets the value loop order.</summary>
    public DxLoopOrder LoopOrder { get; init; }

    /// <summary>Gets the per value offset.</summary>
    public double Offset { get; init; }

    /// <summary>Gets the per value scale.</summary>
    public double Scale { get; init; } = 1;

    /// <summary>Gets the source the binary stream can be read from.</summary>
    /// <remarks>
    /// <param>This shall be a filename without path or folder information to read the data from the file the main description is read from or a full qualified
    /// resource link (protocol://[user[:password]@]server[/path[/..]][?option[=value]]).</param>
    /// <para>This is exclusive with <see cref="Content"/>.</para>
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>Gets the format information needed to read <see cref="Source"/>.</summary>
    /// <remarks>This has to be set if <see cref="Source"/> is set.</remarks>
    public DxBinaryFormat SourceFormat { get; init; }

    /// <summary>Gets the number of raw bytes per value.</summary>
    /// <remarks>This taskes <see cref="Format"/> into account but is independant from <see cref="SourceFormat"/>.</remarks>
    public int BytesPerValue => (int)Format >> 8;

    /// <summary>Gets the number of values at the binary stream.</summary>
    public long ValueCount { get; init; }

    /// <summary>Gets the number of bytes at the binary stream.</summary>
    public long ByteSize => SourceFormat switch
    {
        DxBinaryFormat.Raw => ValueCount * BytesPerValue,
        DxBinaryFormat.BinBlock => DxExtensions.GetBinBlockLength(ValueCount * BytesPerValue),
        _ => -1,
    };

    /// <summary>Gets a value indicating whether the content is a one dimensional array.</summary>
    public bool IsArray => Dimensions.Count == 1;

    /// <summary>Gets a value indicating whether the content is a two dimensional table or matrix.</summary>
    public bool IsTable => Dimensions.Count == 2;

    /// <summary>Gets a value indicating whether the content is a multi dimensional matrix.</summary>
    public bool IsMulti => Dimensions.Count > 2;
}
