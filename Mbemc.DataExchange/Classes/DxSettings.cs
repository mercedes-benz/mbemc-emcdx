// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange;

/// <summary>Provides settings for reading and writing the Mercedes-Benz EMC Data Exchange Format.</summary>
public record DxSettings
{
    string Validate(string fileName)
    {
        if (WorkingFolder is null) throw new AccessViolationException("Streaming context without WorkingFolder may not access files!");
        if (!Path.IsPathRooted(fileName)) fileName = Path.GetFullPath(Path.Combine(WorkingFolder, fileName));
        var resultFileName = Path.GetFullPath(fileName) ?? throw new InvalidOperationException("Result filename cannot be determined!");
        var resultFolder = Path.GetDirectoryName(resultFileName) ?? throw new InvalidOperationException("Result folder cannot be determined!");
        return (Path.GetFullPath(WorkingFolder) == resultFolder) || AllowedFolders.Contains(resultFolder) ? resultFileName : throw new InvalidOperationException($"FileName {fileName} is not within allowed folder structure!");
    }

    /// <summary>Creates a new unique nonexistant temp file with exclusive access (if supported by the filesystem) at the <see cref="WorkingFolder"/>.</summary>
    /// <returns>Returns a new <see cref="FileStream"/> instance with the exclusively opened temp file.</returns>
    internal FileStream CreateFile(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);
        if (name == string.Empty || UniqueFileNames)
        {
            var unique = Guid.NewGuid().ToString();
            fileName = $"{name}.{unique}{ext}";
        }

        fileName = Validate(fileName);
        return File.Exists(fileName)
            ? throw new InvalidOperationException($"{fileName} already exists!")
            : new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
    }

    /// <summary>Gets or sets a value indicating whether unique filenames shall be created when writing files.</summary>
    public virtual bool UniqueFileNames { get; init; }

    /// <summary>
    /// Gets or sets the value count threshold. When using <see cref="DxDataPlacement.Autoselect"/> this threshold decides whether values are stored as binary
    /// or within the json file. (Default = 100)
    /// </summary>
    public virtual ushort ValueCountLimit { get; init; } = 100;

    /// <summary>Gets or sets the folder the storage is working on.</summary>
    public virtual string? WorkingFolder { get; init; }

    /// <summary>Gets or sets the folders the storage may read and write to while reading/writing referenced content/binary files.</summary>
    public DxFolderList AllowedFolders { get; init; } = new();

    /// <summary>Gets a filename based on the specified <paramref name="source"/>.</summary>
    /// <param name="source">Source of the file.</param>
    /// <returns>Returns the full file path and name</returns>
    public virtual string GetFileName(string source) => Validate(source);

    /// <summary>Function to read sources specified at a <see cref="DxDocument"/>.</summary>
    public virtual byte[] ReadSource(string source) => File.ReadAllBytes(GetFileName(source));

    /// <summary>Provides access to the json de-/serializer settings.</summary>
    public DxJson Json { get; init; } = new();

    /// <summary>Binary source format. Defaults to <see cref="DxBinaryFormat.Raw"/>.</summary>
    /// <remarks>This allows to control the storage format of the binary block containing all values.</remarks>
    public DxBinaryFormat BinaryFormat { get; init; } = DxBinaryFormat.Raw;

    /// <summary>
    /// Storage value type to be used for data arrays. This can be overridden by <see cref="DxValueContentAttribute.Placement"/>. Default value is
    /// <see cref="DxDataPlacement.Autoselect"/> using <see cref="ValueCountLimit"/>.
    /// </summary>
    /// <remarks>This allows to control the placement of the underlying value arrays.</remarks>
    public DxDataPlacement Placement { get; init; } = DxDataPlacement.Autoselect;

    /// <summary>Gets the <see cref="DxLoopOrder"/> to use for variable instances.</summary>
    /// <remarks>This allows to control how multi dimensional arrays, lists and matrices are stored.</remarks>
    public DxLoopOrder LoopOrder { get; set; } = DxLoopOrder.Lex;

    /// <summary>Gets the <see cref="DxValueFormat"/> to use for the variable instance.</summary>
    /// <remarks>This allows to control the storage format of each value. This requires <see cref="Placement"/> to be set to <see cref="DxDataPlacement.BinaryFile"/>.</remarks>
    public DxValueFormat ValueFormat { get; set; } = DxValueFormat.Undefined;

    /// <summary>
    /// The maximum supported file size for .emcdx files.
    /// </summary>
    private long _maxEmcdxFileSize = 1L << 27;

    /// <summary>Gets or sets the maximum supported file size for .emcdx files.</summary>
    /// <remarks>Due to .NET restrictions the value cannot be set to a number greater than 2GB.</remarks>
    public long MaxEmcdxFileSize
    {
        get => _maxEmcdxFileSize;
        set
        {
            if (value is < 0 or > (1L << 31))
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)}", "Only values in [0..2GB] are supported.");
            }
            _maxEmcdxFileSize = value;
        }
    }

    /// <summary>
    /// The maxium supported file size for .bin and .txt files
    /// </summary>
    private long _maxBinFileSize = 1L << 31;

    /// <summary>Gets or sets the maximum supported file size for .bin and .txt files.</summary>
    /// <remarks>Due to .NET restrictions the value cannot be set to a number greater than 2GB.</remarks>
    public long MaxBinFileSize
    {
        get => _maxBinFileSize;
        set
        {
            if (value is < 0 or > (1L << 31))
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)}", $"Only values in [0..2GB] are supported. Found {value}");
            }
            _maxBinFileSize = value;
        }
    }

    /// <summary>Approximates the file size of the <paramref name="document"/> if it were to be serialized and throws an exception if it is larger than <see cref="MaxEmcdxFileSize"/></summary>
    /// <param name="document">The document to check</param>
    /// <exception cref="ArgumentOutOfRangeException">If the estimated file size exceeds the limit of <see cref="MaxEmcdxFileSize"/></exception>
    /// <remarks>
    /// For large amounts of data use <see cref="DxDataPlacement.BinaryFile"/>.\n This method checks only the size of the .emcdx file. The size of any
    /// accompanying .bin or .txt files is ignored.
    /// </remarks>
    public void ValidateFileSizeOf(DxDocument document)
    {
        if (EstimateEmcdxFileSize(document) > MaxEmcdxFileSize)
        {
            throw new ArgumentOutOfRangeException($"{nameof(document)}", $"The estimated file size exceeded the maximum of {MaxEmcdxFileSize}");
        }
    }

    /// <summary>Estimates the size in bytes, if the <paramref name="document"/> is about to be serialized.</summary>
    /// <param name="document"></param>
    /// <returns>A rough estimate in bytes</returns>
    long EstimateEmcdxFileSize(DxDocument document)
    {
        //additional quotes for string values.
        const int quotes = 2;
        long size = 0;
        size += "{".Length + EstimateLineOverhead(0);

        size += "\"Name\": ".Length + document.Name.Length + EstimateLineOverhead(1) + quotes;
        size += "\"Description\": ".Length + document.Description?.Length + EstimateLineOverhead(1) + quotes ?? 0;
        size += "\"Data\": [".Length + EstimateLineOverhead(1);

        size += "{".Length + EstimateLineOverhead(2);

        foreach (var dataItem in document.Data)
        {
            size += "\"Name\": ".Length + dataItem.Name.Length + EstimateLineOverhead(3) + quotes;
            size += "\"Description\": ".Length + dataItem.Description?.Length + EstimateLineOverhead(3) + quotes ?? 0;
            size += "\"Variables\": [".Length + EstimateLineOverhead(3);

            foreach (var variable in dataItem.Variables)
            {
                size += "{".Length + EstimateLineOverhead(4);
                size += "\"Name\": ".Length + variable.Name.Length + EstimateLineOverhead(5) + quotes;
                if (variable.Values is { } values)
                {
                    size += "\"Unit\": ".Length + variable.Unit?.Length + EstimateLineOverhead(5) + quotes ?? 0;
                    size += "\"Values\": [".Length + EstimateLineOverhead(5);
                    {
                        //max length of double in chars = 24 (see: https://stackoverflow.com/a/1701085)
                        size += values.Count * (24 + EstimateLineOverhead(6));
                    }
                    size += "]".Length + EstimateLineOverhead(5);
                }
                else if (variable.BinaryValues is { } binaryVal)
                {
                    size += "\"BinaryValues\": {".Length + EstimateLineOverhead(5);
                    {
                        size += "\"ByteOrder\": ".Length + binaryVal.ByteOrder.ToString().Length + EstimateLineOverhead(6) + quotes;
                        size += "\"Format\": ".Length + binaryVal.Format.ToString().Length + EstimateLineOverhead(6) + quotes;
                        size += "\"LoopOrder\": ".Length + binaryVal.LoopOrder.ToString().Length + EstimateLineOverhead(6) + quotes;
                        size += "\"Scale\": ".Length + binaryVal.Scale.ToString().Length + EstimateLineOverhead(6);
                        size += "\"Source\": ".Length + binaryVal.Source?.Length + EstimateLineOverhead(6) + quotes ?? 0;
                        size += "\"SourceFormat\": ".Length + binaryVal.SourceFormat.ToString().Length + EstimateLineOverhead(6) + quotes;
                        size += "\"ValueCount\": ".Length + binaryVal.ValueCount.ToString().Length + EstimateLineOverhead(6);
                        size += "\"Dimensions\": [".Length + EstimateLineOverhead(6);
                        {
                            size += binaryVal.Dimensions.Count.ToString().Length + EstimateLineOverhead(7);
                        }
                        size += "]".Length + EstimateLineOverhead(6);
                        size += "\"Content\": ".Length + binaryVal.Content?.Length + EstimateLineOverhead(6) + quotes ?? 0;
                    }
                    size += "}".Length + EstimateLineOverhead(5);
                }
                size += "}".Length + EstimateLineOverhead(4);
            }
            size += "]".Length + EstimateLineOverhead(3);

            size += "}".Length + EstimateLineOverhead(2);

            size += "]".Length + EstimateLineOverhead(1);
        }
        size += "}".Length + EstimateLineOverhead(0);

        return size;
    }

    /// <summary>Estimates the amount of chars a line at a given indentation-level needs</summary>
    /// <param name="level"></param>
    /// <param name="indentWidth"></param>
    /// <returns></returns>
    long EstimateLineOverhead(int level, int indentWidth = 2)
    {
        const int commaLen = 1;
        var newlineLen = Environment.NewLine.Length;
        return (level * indentWidth) + commaLen + newlineLen;
    }
}
