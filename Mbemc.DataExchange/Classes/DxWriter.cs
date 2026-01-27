// SPDX-License-Identifier: MIT
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mbemc.DataExchange;

/// <summary>Provides a writer class for simple write access to the Mercedes-Benz EMC Data Exchange Format.</summary>
public class DxWriter : IDxName, IDxDescription
{
    #region Private Fields

    readonly List<IDxDataWriter> writers = [];
    IList<string> fileNames = [];

    #endregion Private Fields

    #region Private Methods

    IList<DxDataItem> GetDataItems()
    {
        var binder = new Dictionary<string, DxDataItem>();
        foreach (var item in writers.Select(w => w.ToDataItem()))
        {
            if (!binder.TryGetValue(item.Name, out var dataItem))
            {
                binder[item.Name] = item;
                continue;
            }
            dataItem = binder[item.Name] = new DxDataItem()
            {
                Description = dataItem.Description ?? item.Description,
                Name = dataItem.Name,
                Mappings = [.. dataItem.Mappings, .. item.Mappings],
                Variables = [.. dataItem.Variables, .. item.Variables],
            };
            //enforce unique variable names
            dataItem.Variables.ToDictionary(v => v.Name);
            //enforce unique mapping names
            dataItem.Mappings.ToDictionary(v => v.Name);
        }
        return [.. binder.Values];
    }

    #endregion Private Methods

    #region Public Constructors

    /// <summary>Creates a writer for the mbemc data exchange format.</summary>
    public DxWriter()
    {
        Settings = new DxSettings();
        Name = Guid.NewGuid().ToString();
    }

    /// <summary>Creates a writer for the mbemc data exchange format.</summary>
    /// <param name="settings">Settings to use.</param>
    /// <param name="name">Name to be set at the toplevel of the exchange file.</param>
    /// <param name="description">Description to be set at the toplevel of the exchange file.</param>
    public DxWriter(DxSettings? settings = null, string? name = null, string? description = null)
    {
        Settings = settings ?? new DxSettings();
        Name = name ?? Guid.NewGuid().ToString();
        Description = description;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <summary>Gets the <see cref="DxDocument"/> instance written by the <see cref="Save(string?, JsonDocument?)"/> function.</summary>
    /// <returns>Returns a <see cref="DxDocument"/> instance or null.</returns>
    public DxDocument? Document { get; private set; }

    /// <summary>Gets or inits the meta part of the emcdx file</summary>
    public JsonDocument? Meta { get; init; }

    /// <summary>Gets the <see cref="DxFile"/> instance written by the <see cref="Save(string?, JsonDocument?)"/> function.</summary>
    /// <returns>Returns a <see cref="DxFile"/> instance or null.</returns>
    public DxFile? File { get; private set; }

    /// <summary>Gets the list of files written by the <see cref="Save(string?, JsonDocument?)"/> function.</summary>
    public IList<string> FileNames => new ReadOnlyCollection<string>(fileNames);

    /// <inheritdoc/>
    public string Name { get; init; }

    /// <summary>Gets the settings used by the writer.</summary>
    public DxSettings Settings { get; } = new();

    #endregion Public Properties

    #region Public Methods

    /// <summary>Creates a writer for the data item with the specified <paramref name="name"/>.</summary>
    /// <remarks>
    /// It is possible to create multiple writers, writing different <typeparamref name="TDataset"/> (but not the same!) types to a single data item.
    /// </remarks>
    /// <typeparam name="TDataset">Dataset to write. The properties need to be marked using the <see cref="DxVariableAttribute"/> and <see cref="DxMappingAttribute"/>.</typeparam>
    /// <param name="name">Name of the data item to create.</param>
    /// <param name="description">Description of the data item to create.</param>
    /// <param name="fileNamePrefix">Filename used as prefix for binary data</param>
    /// <returns>Returns a new <see cref="DxDataWriter"/> instance able to write the datasets.</returns>
    public DxDataWriter CreateDataItem<TDataset>(string? name = null, string? description = null, string? fileNamePrefix = null)
        where TDataset : class
    {
        name ??= typeof(TDataset).Name;
        fileNamePrefix ??= DxExtensions.CreateFileName($"{Name}-{name}");
        var writer = DxDataWriter.CreateTyped<TDataset>(Settings, name, description, fileNamePrefix);
        writers.Add(writer);
        return writer;
    }

    /// <summary>Creates a writer for the data item with the specified <paramref name="name"/>.</summary>
    /// <param name="name">Name of the data item to create.</param>
    /// <param name="description">Description of the data item to create.</param>
    /// <param name="fileNamePrefix">Filename used as prefix for binary data</param>
    /// <returns>Returns a new <see cref="DxDataWriter"/> instance able to write the datasets.</returns>
    public DxDataWriter CreateDataItem(string name, string? description = null, string? fileNamePrefix = null)
    {
        fileNamePrefix ??= DxExtensions.CreateFileName($"{Name}-{name}");
        var writer = DxDataWriter.CreateUntyped(Settings, name, description, fileNamePrefix);
        writers.Add(writer);
        return writer;
    }

    /// <summary>Saves all written data to the specified file.</summary>
    /// <remarks>This function uses the Settings.WorkingFolder to write the files to.</remarks>
    /// <param name="fileName">FileName to write to</param>
    /// <param name="metadata">Optional json tree to add custom properties to the file.</param>
    public void Save(string? fileName = null, JsonDocument? metadata = null)
    {
        fileName ??= DxExtensions.CreateFileName(Name) + ".emcdx";
        if (Document is not null) throw new InvalidOperationException("Document was already written!");
        Document = new() { Data = GetDataItems(), Name = Name, Description = Description, Meta = metadata };
        File = Document.ToFile();
        Settings.ValidateFileSizeOf(File);
        using var fileStream = Settings.CreateFile(fileName);
        Settings.Json.Save(fileStream, Document);
        var files = new List<string>
        {
            fileName
        };
        foreach (var writer in writers)
        {
            files.AddRange(writer.FileNames);
            writer.Close();
        }
        fileNames = new ReadOnlyCollection<string>([.. files.Select(Settings.GetFileName)]);
    }

    /// <summary>Writes a number of datasets to new data items at the exchange file.</summary>
    /// <remarks>
    /// This should be only called once per name (or type if name is not set), since each call to this function creates new data items for the specified <paramref name="item"/>.
    /// </remarks>
    /// <typeparam name="TDataset">Dataset type</typeparam>
    /// <param name="item">Item to write.</param>
    public void Write<TDataset>(TDataset item)
        where TDataset : class, IDxName, IDxDescription
    {
        var type = typeof(TDataset);
        var name = (item is IDxName n) ? n.Name : null ?? type.Name;
        var description = (item is IDxDescription d) ? d.Description : null;
        var dataItem = CreateDataItem<TDataset>(name, description);
        dataItem.Write(item);
    }

    /// <summary>Writes a number of datasets to new data items at the exchange file.</summary>
    /// <remarks>
    /// This should be only called once per name (or type if name is not set), since each call to this function creates new data items for the specified <paramref name="items"/>.
    /// </remarks>
    /// <typeparam name="TDataset">Dataset type</typeparam>
    /// <param name="items">Items to write.</param>
    public void Write<TDataset>(IEnumerable<TDataset> items)
        where TDataset : class, IDxName, IDxDescription
    {
        foreach (var set in items.ToLookup(i => i.Name))
        {
            var name = set.Key;
            var dataItem = CreateDataItem<TDataset>(name, set.First().Description);
            foreach (var item in set)
            {
                dataItem.Write(item);
            }
        }
    }

    #endregion Public Methods
}
