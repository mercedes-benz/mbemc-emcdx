// SPDX-License-Identifier: MIT
using System.Globalization;
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestDirectWriting : TestWithClearWorkingFolder
{
    #region Private Fields

    readonly string testFolder;

    #endregion Private Fields

    #region Public Constructors

    public TestDirectWriting()
    {
        //start at current folder, move up until project main folder is found
        testFolder = ".";
        while (!File.Exists(Path.Combine(testFolder, "Mbemc.DataExchange.slnx")))
        {
            var oneFolderUp = Path.GetFullPath(Path.Combine(testFolder, ".."));
            if (oneFolderUp == testFolder) throw new DirectoryNotFoundException("Testdata not found!");
            testFolder = oneFolderUp;
        }
        testFolder = Path.GetFullPath(Path.Combine(testFolder, "Samples/CSV"));
        if (!Directory.Exists(testFolder)) throw new DirectoryNotFoundException("Testdata not found!");
    }

    #endregion Public Constructors

    #region Public Methods

    [Fact]
    public void TestImportCSV()
    {
        var lines = File.ReadAllLines(Path.Combine(testFolder, "SampleMeasurement01.csv")) ?? throw new InvalidOperationException("Empty data!");

        var header = lines[0].Split(';') ?? throw new InvalidOperationException("Empty header (line 0 of csv)!");
        var units = lines[1].Split(';') ?? throw new InvalidOperationException("Empty units (line 1 of csv)!");
        var names = header.Select(DxExtensions.CreateFileName).ToList();

        if (units.Length != header.Length) throw new InvalidOperationException("Column title count in line 1 of csv does not match unit count in line 2!");

        //create variables from csv header
        var variables = new DxVariable[units.Length];
        {
            for (var n = 0; n < units.Length; n++)
            {
                //in this csv unwanted columns are marked with unit = --- or empty
                switch (units[n])
                {
                    case null:
                    case "":
                    case "---": continue;
                }

                variables[n] = new DxVariable()
                {
                    Name = names[n],
                    Unit = units[n],
                    Values = new List<double>(lines.Length),
                };
            }
        }

        //read data into variables
        for (var i = 2; i < lines.Length; i++)
        {
            var cells = lines[i].Split(';');
            if (cells.Length != units.Length) throw new InvalidOperationException($"Column count in line {i + 1} of csv does not match unit count in line 2!");
            for (var n = 0; n < units.Length; n++)
            {
                variables[n]?.Values!.Add(double.Parse(cells[n], CultureInfo.InvariantCulture));
            }
        }

        //create writer with unique filenames
        var writer = new DxWriter(TestSettings, name: "SampleMeasurement01", description: "Import of SampleMeasurement01.csv");
        var data = writer.CreateDataItem("SampleMeasurement01");
        foreach (var variable in variables)
        {
            if (variable is not null) data.WriteVariable(variable);
        }
        //write the file
        writer.Save();
    }

    #endregion Public Methods
}
