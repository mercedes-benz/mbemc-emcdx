// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

public class TestWithClearWorkingFolder : IDisposable
{
    #region Private Fields

    bool disposedValue;

    #endregion Private Fields

    #region Private Destructors

    ~TestWithClearWorkingFolder() => Dispose(disposing: false);

    #endregion Private Destructors

    #region Private Methods

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            try
            {
                if (TestSettings.WorkingFolder is not null)
                {
                    Directory.Delete(TestSettings.WorkingFolder, true);
                }
            }
            catch { }
            disposedValue = true;
        }
    }

    #endregion Private Methods

    #region Public Constructors

    public TestWithClearWorkingFolder()
    {
        TestSettings = new() { WorkingFolder = Tools.CreateTemp() };
    }

    #endregion Public Constructors

    #region Public Properties

    public DxSettings TestSettings { get; }

    #endregion Public Properties

    #region Public Methods

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Public Methods
}
