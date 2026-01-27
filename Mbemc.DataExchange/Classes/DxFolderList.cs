// SPDX-License-Identifier: MIT
using System.Data;

namespace Mbemc.DataExchange;

/// <summary>Provides a list of (unique and present) folders.</summary>
public class DxFolderList : ICollection<string>
{
    #region Private Fields

    readonly HashSet<string> folders = new();

    #endregion Private Fields

    #region Private Methods

    static string Validate(string folder)
    {
        folder = Path.GetFullPath(folder);
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException($"Folder {folder} not found!");
        return folder;
    }

    #endregion Private Methods

    #region Public Constructors

    /// <summary>Creates a new empty instance of the <see cref="DxFolderList"/> class.</summary>
    public DxFolderList() { }

    /// <summary>Creates a new instance of the <see cref="DxFolderList"/> class and adds all specified <paramref name="folders"/> after validation.</summary>
    /// <param name="folders">Folder to add to the list</param>
    /// <param name="setReadonly">Set the list readonly after adding the folders.</param>
    public DxFolderList(IEnumerable<string> folders, bool setReadonly = false)
    {
        AddRange(folders);
        IsReadOnly = setReadonly;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <inheritdoc/>
    public int Count => ((ICollection<string>)folders).Count;

    /// <inheritdoc/>
    public bool IsReadOnly { get; private set; }

    #endregion Public Properties

    #region Public Methods

    /// <inheritdoc/>
    public void Add(string folder)
    {
        if (IsReadOnly) throw new ReadOnlyException();
        folders.Add(Validate(folder));
    }

    /// <summary>Adds a number of folders to the folder list.</summary>
    /// <param name="folders">Folders to validate and add.</param>
    public void AddRange(IEnumerable<string> folders)
    {
        foreach (var folder in folders)
        {
            Add(folder);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (IsReadOnly) throw new ReadOnlyException();
        folders.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(string folder) => folders.Contains(Validate(folder));

    /// <inheritdoc/>
    public void CopyTo(string[] array, int arrayIndex) => folders.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator() => folders.GetEnumerator();

    /// <inheritdoc/>
    public bool Remove(string item) => folders.Remove(Validate(item));

    /// <summary>Sets the list readonly.</summary>
    public void SetReadOnly() => IsReadOnly = true;

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((System.Collections.IEnumerable)folders).GetEnumerator();

    #endregion Public Methods
}
