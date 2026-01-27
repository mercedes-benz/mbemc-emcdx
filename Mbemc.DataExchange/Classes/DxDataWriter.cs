// SPDX-License-Identifier: MIT
using System.Diagnostics;
using System.Reflection;

namespace Mbemc.DataExchange;

/// <summary>Provides a writer class able to create a <see cref="DxDataItem"/> by writing records.</summary>
[DebuggerDisplay("DxDataWriter Name = {Name}")]
public class DxDataWriter : IDxDataWriter, IDxDescription, IDxName
{
    #region Private Fields

    /// <summary>named list of generated variables</summary>
    readonly Dictionary<string, DxVariable> generators = [];

    /// <summary>list of writers for variables and generator</summary>
    readonly List<DxPropertyWriter> writers = [];

    #endregion Private Fields

    #region Private Constructors

    DxDataWriter(DxSettings settings, string? name = null, string? description = null, string? fileNamePrefix = null)
    {
        Name = name ?? Guid.NewGuid().ToString();
        FileNamePrefix = fileNamePrefix ?? Name;
        Settings = settings;
        Description = description;

        if (!DxExtensions.IsValidFileName(FileNamePrefix)) throw new InvalidOperationException($"FileNamePrefix {FileNamePrefix} is invalid!");
    }

    #endregion Private Constructors

    #region Private Methods

    void CheckDuplicateName(string name)
    {
        if (writers.Any(w => w.Name == name) || generators.Values.Any(g => g.Name == name)) throw new InvalidOperationException($"Duplicate Item.Name {name}!");
    }

    IList<DxMapping> GetMappings() => [.. writers.Where(w => w.IsMapping).Select(w => w.GetMapping())];

    IList<DxVariable> GetVariables() => [.. generators.Values, .. writers.Where(w => w.IsVariable).Select(w => w.GetVariable())];

    #endregion Private Methods

    #region Public Properties

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <summary>Prefix for binary files</summary>
    public string FileNamePrefix { get; }

    /// <inheritdoc/>
    public IList<string> FileNames => [.. writers.Where(w => w.UseFile).Select(w => w.FileName!)];

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>Gets the settings for XChange format generation.</summary>
    public DxSettings Settings { get; }

    /// <summary>Gets the variable names.</summary>
    public IList<string> VariableNames => [.. GetVariables().Select(v => v.Name)];

    #endregion Public Properties

    #region Public Methods

    /// <summary>Creates a new <see cref="DxDataWriter"/> with the specified properties.</summary>
    /// <param name="settings">Settings used for the writer.</param>
    /// <param name="name">Name of the data item to create.</param>
    /// <param name="description">Description of the data item to create.</param>
    /// <param name="fileNamePrefix">Filename used as prefix for binary data.</param>
    public static DxDataWriter CreateTyped<TDataset>(DxSettings settings, string? name = null, string? description = null, string? fileNamePrefix = null)
        where TDataset : class
    {
        var rowType = typeof(TDataset);
        var properties = rowType.GetProperties();
        fileNamePrefix ??= rowType.Name;
        name ??= rowType.Name;
        var writer = CreateUntyped(settings, name, description, fileNamePrefix);
        foreach (var property in properties)
        {
            foreach (var attribute in property.GetCustomAttributes())
            {
                if (attribute is DxGeneratedAttribute generator)
                {
                    var generated = DxVariable.FromProperty(property, generator);
                    if (writer.generators.ContainsKey(property.Name))
                    {
                        throw new InvalidOperationException($"Cannot create multiple generated variables with the same property name (Property = {property})!");
                    }
                    if (writer.generators.Values.Any(v => v.Name == generated.Name))
                    {
                        throw new InvalidOperationException($"Cannot create multiple generated variables with the same name (Name = {generated.Name})!");
                    }
                    writer.generators.Add(property.Name, generated);
                    continue;
                }
                if (attribute is DxMappingAttribute mapping)
                {
                    var itemName = mapping.Name ?? property.Name;
                    var propertyWriter = DxPropertyWriter.Create(settings, property, $"{fileNamePrefix}.{itemName}");
                    writer.writers.Add(propertyWriter);
                    continue;
                }
                if (attribute is DxVariableAttribute variable)
                {
                    var itemName = variable.Name ?? property.Name;
                    var propertyWriter = DxPropertyWriter.Create(settings, property, $"{fileNamePrefix}.{itemName}");
                    writer.writers.Add(propertyWriter);
                    continue;
                }
                Trace.TraceWarning($"Ignore property {property}");
            }
        }
        return writer;
    }

    /// <summary>Creates a new <see cref="DxDataWriter"/> with the specified properties.</summary>
    /// <param name="settings">Settings used for the writer.</param>
    /// <param name="name">Name of the data item to create.</param>
    /// <param name="description">Description of the data item to create.</param>
    /// <param name="fileNamePrefix">Filename used as prefix for binary data.</param>
    public static DxDataWriter CreateUntyped(DxSettings settings, string? name = null, string? description = null, string? fileNamePrefix = null) => new(settings, name, description, fileNamePrefix);

    /// <inheritdoc/>
    public void Close()
    {
        writers.Clear();
        generators.Clear();
    }

    /// <summary>Sets the generator settings for the specified variable.</summary>
    /// <param name="name"></param>
    /// <param name="settings"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public void SetGeneratorSettings(string name, DxValueGeneratorSettings settings)
    {
        var generator = generators.Single(p => p.Key == name || p.Value.Name == name);
        var key = generator.Key;
        generators[key] = generator.Value with { Generator = settings };
    }

    /// <inheritdoc/>
    public DxDataItem ToDataItem() => new()
    {
        Description = Description,
        Name = Name,
        Mappings = GetMappings(),
        Variables = GetVariables(),
    };

    /// <summary>Writes a dataset to the variables and mappings.</summary>
    /// <remarks>
    /// This function is used with typed property writers created by <see cref="CreateTyped{TDataset}(DxSettings, string?, string?, string?)"/>. If you used
    /// untyped writers created by <see cref="CreateUntyped(DxSettings, string?, string?, string?)"/> use <see cref="WriteVariable(DxVariable)"/> and
    /// <see cref="WriteMapping(DxMapping)"/> instead!
    /// </remarks>
    /// <param name="dataset">Dataset to write</param>
    public void Write<TDataset>(TDataset dataset)
    {
        var success = false;
        foreach (var writer in writers)
        {
            if (writer.PropertyInfo is not null)
            {
                var value = writer.PropertyInfo.GetValue(dataset) ?? throw new NullReferenceException($"Property {writer.PropertyInfo} value is set to null!");
                if (writer.ValueCount > 0)
                {
                    writer.AddContent(value);
                }
                else
                {
                    writer.SetContent(value, true);
                }
                success = true;
            }
        }
        if (!success) throw new InvalidOperationException("Could not write dataset. No typed property writer can handle this dataset!");
    }

    /// <summary>Writes datasets to the variables and mappings.</summary>
    /// <remarks>
    /// This function is used with typed property writers created by <see cref="CreateTyped{TDataset}(DxSettings, string?, string?, string?)"/>. If you used
    /// untyped writers created by <see cref="CreateUntyped(DxSettings, string?, string?, string?)"/> use <see cref="WriteVariable(DxVariable)"/> and
    /// <see cref="WriteMapping(DxMapping)"/> instead!
    /// </remarks>
    /// <param name="datasets">Datasets to write</param>
    public void Write<TDataset>(params TDataset[] datasets)
        where TDataset : class
    {
        foreach (var dataset in datasets)
        {
            Write(dataset);
        }
    }

    /// <summary>Writes datasets to the variables and mappings.</summary>
    /// <remarks>
    /// This function is used with typed property writers created by <see cref="CreateTyped{TDataset}(DxSettings, string?, string?, string?)"/>. If you used
    /// untyped writers created by <see cref="CreateUntyped(DxSettings, string?, string?, string?)"/> use <see cref="WriteVariable(DxVariable)"/> and
    /// <see cref="WriteMapping(DxMapping)"/> instead!
    /// </remarks>
    /// <param name="datasets">Datasets to write</param>
    public void Write<TDataset>(IEnumerable<TDataset> datasets)
        where TDataset : class
    {
        foreach (var dataset in datasets)
        {
            Write(dataset);
        }
    }

    /// <summary>Writes a mapping with content.</summary>
    /// <remarks>
    /// This function is used with untyped property writers created by <see cref="CreateUntyped(DxSettings, string?, string?, string?)"/>. If you used typed
    /// writers created by <see cref="CreateTyped{TDataset}(DxSettings, string?, string?, string?)"/> use the <see cref="Write{TDataset}(TDataset)"/> overloads instead!
    /// </remarks>
    /// <param name="mapping">Mapping to write.</param>
    public void WriteMapping(DxMapping mapping)
    {
        CheckDuplicateName(mapping.Name);
        if (mapping.BinaryValues is not null)
        {
            var propertyWriter = DxPropertyWriter.Create(Settings, mapping);
            writers.Add(propertyWriter);
            return;
        }
        if (mapping.Values is not null)
        {
            var propertyWriter = DxPropertyWriter.Create(Settings, mapping);
            writers.Add(propertyWriter);
            propertyWriter.SetContent(mapping.Values!, true);
            return;
        }
        throw new InvalidOperationException($"Mapping {mapping} does not define Values or BinaryValues!");
    }

    /// <summary>Writes a variable with content.</summary>
    /// <remarks>
    /// This function is used with untyped property writers created by <see cref="CreateUntyped(DxSettings, string?, string?, string?)"/>. If you used typed
    /// writers created by <see cref="CreateTyped{TDataset}(DxSettings, string?, string?, string?)"/> use the <see cref="Write{TDataset}(TDataset)"/> overloads instead!
    /// </remarks>
    /// <param name="variable">Variable to write.</param>
    public void WriteVariable(DxVariable variable)
    {
        CheckDuplicateName(variable.Name);

        if (variable.Generator is not null)
        {
            generators.Add(variable.Name, variable);
            return;
        }
        if (variable.BinaryValues is not null)
        {
            var propertyWriter = DxPropertyWriter.Create(Settings, variable);
            writers.Add(propertyWriter);
            return;
        }
        if (variable.Values is not null)
        {
            var propertyWriter = DxPropertyWriter.Create(Settings, variable);
            writers.Add(propertyWriter);
            propertyWriter.SetContent(variable.Values!, true);
            return;
        }
        throw new InvalidOperationException($"Variable {variable} is no Generator and does not define Values or BinaryValues!");
    }

    #endregion Public Methods
}
