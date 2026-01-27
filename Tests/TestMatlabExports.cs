// SPDX-License-Identifier: MIT
using System.Text.Encodings.Web;
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestMatlabExports : IDisposable
{
    #region Private Fields

    readonly DxSettings readerSettings;
    readonly string testFolder;
    readonly DxSettings writerSettings;
    bool disposedValue;

    #endregion Private Fields

    #region Private Destructors

    ~TestMatlabExports() => Dispose(disposing: false);

    #endregion Private Destructors

    #region Private Methods

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            try
            {
                if (writerSettings.WorkingFolder is not null)
                {
                    Directory.Delete(writerSettings.WorkingFolder, true);
                }
            }
            catch { }
            disposedValue = true;
        }
    }

    #endregion Private Methods

    #region Public Constructors

    public TestMatlabExports()
    {
        //start at current folder, move up until project main folder is found
        testFolder = ".";
        while (!File.Exists(Path.Combine(testFolder, "Mbemc.DataExchange.slnx")))
        {
            var oneFolderUp = Path.GetFullPath(Path.Combine(testFolder, ".."));
            if (oneFolderUp == testFolder) throw new DirectoryNotFoundException("Testdata not found!");
            testFolder = oneFolderUp;
        }
        testFolder = Path.GetFullPath(Path.Combine(testFolder, "Samples/WaveForms/Matlab"));
        if (!Directory.Exists(testFolder)) throw new DirectoryNotFoundException("Testdata not found!");

        writerSettings = new() { WorkingFolder = Tools.CreateTemp() };
        readerSettings = new() { AllowedFolders = new([testFolder, testFolder + "/../Binary"]) };
    }

    #endregion Public Constructors

    #region Public Methods

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void TestReadSampleFileManual()
    {
        var files = Directory.GetFiles(testFolder, "*.emcdx");
        Assert.True(files.Length >= 4);
        foreach (var file in files)
        {
            try
            {
                var reader = DxReader.ReadFile(file, readerSettings);
                var document = reader.File;
                var iqData = document.Data.Single(d => d.Name == "IQ");
                var iqRealVariable = iqData.Variables.Single(v => v.Name == "Real_Part");
                var iqImagVariable = iqData.Variables.Single(v => v.Name == "Imag_Part");
                var iqRealValues = reader.LoadValues<double>(iqRealVariable) ?? throw new InvalidDataException("Could not convert IQ Real");
                var iqImagValues = reader.LoadValues<double>(iqImagVariable) ?? throw new InvalidDataException("Could not convert IQ Imag");
                if (iqRealValues.Count != iqImagValues.Count) throw new InvalidDataException($"IQ Real Count {iqRealValues.Count} does not match Imag Count {iqImagValues.Count}!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading sample file {file}", ex);
            }
        }
    }

    [Fact]
    public void TestReadWriteSampleFile()
    {
        var files = Directory.GetFiles(testFolder, "*.emcdx");
        Assert.True(files.Length >= 4);
        foreach (var file in files)
        {
            var reader = DxReader.ReadFile(file, readerSettings);
            var iqData = reader.Read<IQData>().Single();
            Assert.Multiple(() =>
            {
                Assert.Equal("IQ", iqData.Name);
                Assert.Equal("IQ data", iqData.Description);
                Assert.Equal(iqData.Real.Count, iqData.Imaginary.Count);
            });

            IList<string> resultFiles;

            // Write iq data with default settings
            {
                var writer = new DxWriter(writerSettings, name: Path.GetFileNameWithoutExtension(file), description: reader.File.Description);
                writer.Write(iqData);
                writer.Save();
                resultFiles = writer.FileNames;
                Assert.Equal(3, resultFiles.Count);
            }

            // roundtrip: read written data without special settings
            {
                var reader2 = DxReader.ReadFile(resultFiles.First());
                var iqData2 = reader2.Read<IQData>().Single();

                Assert.Multiple(() =>
                {
                    Assert.Equal(iqData.PeakToRootMeanSquare, iqData2.PeakToRootMeanSquare);
                    Assert.Equal(iqData.Real, iqData2.Real);
                    Assert.Equal(iqData.Imaginary, iqData2.Imaginary);
                    Assert.Equal(iqData.SampleRate, iqData2.SampleRate);
                    Assert.Equal(iqData.Description, iqData2.Description);
                    Assert.Equal(iqData.Name, iqData2.Name);
                    Assert.Equal(iqData.PeakOffset, iqData2.PeakOffset);
                    Assert.Equal(iqData.PeakToAverage, iqData2.PeakToAverage);
                    Assert.Equal(iqData.RootMeanSquareOffset, iqData2.RootMeanSquareOffset);
                });
            }

            // Write iq data embedded in different formats, with unsafe characters (only supported up to medium sized data)
            if (iqData.Real.Count < 1000000)
            {
                foreach (var binaryFormat in new DxBinaryFormat[] { DxBinaryFormat.BinBlock, DxBinaryFormat.Base64, DxBinaryFormat.Text })
                {
                    var sourceType = DxDataPlacement.Content;
                    {
                        var writer2 = new DxWriter(
                            name: Path.GetFileNameWithoutExtension(file) + $"_embedded_{binaryFormat}",
                            description: reader.File.Description,
                            settings: writerSettings with
                            {
                                Json = writerSettings.Json with { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping },
                                ValueCountLimit = 0,
                                Placement = sourceType,
                                BinaryFormat = binaryFormat
                            });

                        writer2.Write(iqData);
                        writer2.Save();
                        resultFiles = writer2.FileNames;
                        Assert.Single(resultFiles);
                    }

                    // roundtrip: read written data without special settings
                    {
                        var reader2 = DxReader.ReadFile(resultFiles.First());
                        var iqData2 = reader2.Read<IQData>().Single();

                        Assert.Multiple(() =>
                        {
                            Assert.Equal(iqData.PeakToRootMeanSquare, iqData2.PeakToRootMeanSquare);
                            Assert.Equal(iqData.Real, iqData2.Real);
                            Assert.Equal(iqData.Imaginary, iqData2.Imaginary);
                            Assert.Equal(iqData.SampleRate, iqData2.SampleRate);
                            Assert.Equal(iqData.Description, iqData2.Description);
                            Assert.Equal(iqData.Name, iqData2.Name);
                            Assert.Equal(iqData.PeakOffset, iqData2.PeakOffset);
                            Assert.Equal(iqData.PeakToAverage, iqData2.PeakToAverage);
                            Assert.Equal(iqData.RootMeanSquareOffset, iqData2.RootMeanSquareOffset);
                        });
                    }
                }
            }
        }
    }

    #endregion Public Methods
}
