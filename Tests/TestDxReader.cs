// SPDX-License-Identifier: MIT
using System.Text;
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestDxReader
{
    #region Private Fields

    readonly DxReader reader;

    readonly string validEmcdx = @"{""Type"": ""EMC Data Exchange Format"",""Version"": ""2.0"",""Name"": ""Beispieldatei"",""Data"": {}}";

    #endregion Private Fields

    #region Public Constructors

    public TestDxReader()
    {
        var stream = new MemoryStream();

        var bytes = Encoding.UTF8.GetBytes(validEmcdx);
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        reader = DxReader.ReadStream(stream);
    }

    #endregion Public Constructors

    #region Public Methods

    [Fact]
    public void TestReadStreamWithoutSettings()
    {
        Assert.Multiple(() =>
        {
            Assert.Null(reader.Settings.WorkingFolder);
            Assert.Equal("EMC Data Exchange Format", reader.File.Type);
            Assert.Equal(new Version(2, 0), reader.File.Version);
            Assert.Equal("Beispieldatei", reader.File.Name);
            Assert.Single(reader.File.Data);
        });
    }

    #endregion Public Methods
}
