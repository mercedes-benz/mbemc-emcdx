// SPDX-License-Identifier: MIT

namespace Mbemc.DataExchange;

/// <summary>Provides a reader class for simple read access of the Mercedes-Benz EMC Data Exchange Format.</summary>
public class DxReader
{
    #region Private Constructors

    /// <summary>Creates a new instance of the <see cref="DxReader"/> class reading from <paramref name="file"/>.</summary>
    /// <param name="source">Source of the file</param>
    /// <param name="file">File to read from.</param>
    /// <param name="settings">Settings for the reader.</param>
    DxReader(string? source, DxFile file, DxSettings settings)
    {
        Source = source;
        Settings = settings;
        File = file;
    }

    #endregion Private Constructors

    #region Private Methods

    DxDataReader<TDataset> GetReader<TDataset>() where TDataset : class, new() => new DxDataReader<TDataset>(this);

    #endregion Private Methods

    #region Public Properties

    /// <summary>Gets the <see cref="DxFile"/> instance.</summary>
    public DxFile File { get; }

    /// <summary>Gets the settings used by the reader.</summary>
    public DxSettings Settings { get; }

    /// <summary>Gets the source of the file (this is used when loading content/binary sources).</summary>
    public string? Source { get; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>Creates a new instance of the <see cref="DxReader"/> class reading from <paramref name="fileName"/>.</summary>
    /// <param name="fileName">File to read from</param>
    /// <param name="settings">Settings for the reader</param>
    public static DxReader ReadFile(string fileName, DxSettings? settings = null)
    {
        fileName = Path.GetFullPath(fileName);
        settings ??= new();
        settings = settings with { WorkingFolder = Path.GetDirectoryName(fileName) ?? "." };
        using var stream = new MemoryStream(settings.ReadSource(fileName));
        var file = settings.Json.Load<DxFile>(stream);
        return new DxReader(fileName, file, settings);
    }

    /// <summary>Creates a new instance of the <see cref="DxReader"/> class reading from <paramref name="stream"/>.</summary>
    /// <param name="stream">Stream to read from.</param>
    /// <param name="settings">
    /// Settings for the reader. Overload the <see cref="DxSettings"/> record to implement <see cref="DxSettings.ReadSource(string)"/> to enable reading binary
    /// data from foreign streams.
    /// </param>
    public static DxReader ReadStream(Stream stream, DxSettings? settings = null)
    {
        settings ??= new() { Json = new() { } };
        settings = settings with { WorkingFolder = null };
        var file = settings.Json.Load<DxFile>(stream);
        return new DxReader(null, file, settings);
    }

    /// <summary>Loads the values of the specified <paramref name="item"/>.</summary>
    /// <typeparam name="TValue">Target type.</typeparam>
    /// <param name="item">Item to read</param>
    /// <returns>Returns a new array of values.</returns>
    /// <exception cref="InvalidOperationException">Could load values of item {item}!</exception>
    public IList<TValue> LoadValues<TValue>(DxItemWithValues item) where TValue : struct
        => item.Values?.Cast<TValue>().ToList() ?? Settings.LoadValues<TValue>(item.BinaryValues) ?? throw new InvalidOperationException($"Could not load values of item {item}!");

    /// <summary>Reads <typeparamref name="TDataset"/> instances.</summary>
    /// <typeparam name="TDataset"></typeparam>
    /// <returns>Returns an <see cref="IEnumerable{TDataset}"/>.</returns>
    public IEnumerable<TDataset> Read<TDataset>(Func<DxDataItem, bool>? predicate = null)
        where TDataset : class, new()
    {
        var reader = GetReader<TDataset>();
        var result = reader.Read(predicate);
        return result;
    }

    #endregion Public Methods
}
