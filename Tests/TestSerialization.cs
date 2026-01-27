// SPDX-License-Identifier: MIT
using Xunit;

namespace Mbemc.DataExchange.Tests;

public class TestSerialization : TestWithClearWorkingFolder
{
    #region Public Methods

    [Fact]
    public void TestWrite1()
    {
        var generatorSettings2 = new DxValueGeneratorSettings()
        {
            Start = -250,
            Length = 5e+5m,
            Step = 2e-1m,
            Mode = DxValueGeneratorMode.Linear
        };
        var measurementResultsPlug2 = new DxValueGenerator(generatorSettings2).Select(t => new Plug2()
        {
            TimeStamp = t,
            Current1 = Math.Sin(t / 25000),
            Current2 = Math.Abs(Math.Sin(t / 25000)),
            Voltage = Math.Cos(t / 50000),
        });

        var generatorSettings1 = new DxValueGeneratorSettings()
        {
            Start = -125,
            Length = 500,
            Step = 20,
            Mode = DxValueGeneratorMode.Linear
        };
        var measurementResultsPlug1 = new DxValueGenerator(generatorSettings1).Select(t => new Plug1()
        {
            TimeStamp = t,
            GRA = Math.Sin(t / 2.5)
        });

        //create writer
        var writer = new DxWriter(TestSettings);

        //add measurement 1 - plug 1 definition
        var plug1 = writer.CreateDataItem<Plug1>("Messung 1");
        //set generator or add variables
        plug1.SetGeneratorSettings(nameof(Plug1.TimeStamp), generatorSettings1);
        //write the plug 1 readings
        plug1.Write(measurementResultsPlug1);

        //add measurement 1 - plug 2 readings
        var plug2 = writer.CreateDataItem<Plug2>("Messung 1");
        //set generator or add variables
        plug2.SetGeneratorSettings(nameof(Plug2.TimeStamp), generatorSettings2);
        //write the plug 2 readings
        plug2.Write(measurementResultsPlug2);

        //write the file
        writer.Save();
    }

    [Fact]
    public void TestWrite2()
    {
        var generatorSettings2 = new DxValueGeneratorSettings()
        {
            Start = -250,
            Length = 5e+5m,
            Step = 2e-1m,
            Mode = DxValueGeneratorMode.Linear
        };
        var measurementResultsPlug2 = new DxValueGenerator(generatorSettings2).Select(t => new Plug2()
        {
            TimeStamp = t,
            Current1 = Math.Sin(t / 25000),
            Current2 = Math.Abs(Math.Sin(t / 25000)),
            Voltage = Math.Cos(t / 50000),
        });

        var generatorSettings1 = new DxValueGeneratorSettings()
        {
            Start = -125,
            Length = 500,
            Step = 20,
            Mode = DxValueGeneratorMode.Linear
        };
        var measurementResultsPlug1 = new DxValueGenerator(generatorSettings1).Select(t => new Plug1()
        {
            TimeStamp = t,
            GRA = Math.Sin(t / 2.5)
        });

        //create writer with unique filenames
        var writer = new DxWriter(TestSettings);

        //add measurement 1 - plug 1 definition
        var plug1 = writer.CreateDataItem<Plug1>("Messung 2");
        //set generator or add variables
        plug1.SetGeneratorSettings(nameof(Plug1.TimeStamp), generatorSettings1);
        //write the plug 1 readings
        plug1.Write(measurementResultsPlug1);

        //add measurement 1 - plug 2 readings
        var plug2 = writer.CreateDataItem<Plug2>("Messung 2");
        //set generator or add variables
        plug2.SetGeneratorSettings(nameof(Plug2.TimeStamp), generatorSettings2);
        //write the plug 2 readings
        plug2.Write(measurementResultsPlug2);

        //write the file
        writer.Save();
    }

    #endregion Public Methods
}
