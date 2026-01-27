// SPDX-License-Identifier: MIT

/*
* Program for analyzing the MBN50482 test waveforms
*
* This program enables the processing and analysis of the waveforms saved at the emcdx file format,
* which are used in tests with IQ signal generators according to the MBN50482 standard.
*
* The signals are read from the matlab export to be found at Samples/WaveForms/Matlab.
*
* Each signal is read and visualized using the oxyplot library.
*
* There are three plots per signal.
*
* In time domain magnitude and phase are plotted over time.
* If a signal is shorter than 10ms it is duplicated until it is longer than 10ms.
* BLN signals are truncated to 10ms.
*
* The third plot is a frequency spectrum of each signal.
* You can compare the results to the plots of the matlab functions used to generate the signals
* at Samples/WaveForms/Plots
*
* The expected output can be found at Samples/WaveForms/Oxyplot
*/

using Mbemc.DataExchange;

namespace MBN50284;

internal class Program
{
    #region Private Methods

    static string FindSampleFolder()
    {
        var current = Directory.GetCurrentDirectory();
        while (true)
        {
            if (Directory.Exists(Path.Combine(current, "WaveForms/Matlab")))
            {
                return Path.GetFullPath(Path.Combine(current, "WaveForms/Matlab"));
            }
            var last = current;
            current = Path.GetFullPath(Path.Combine(current, ".."));
            if (current == last) throw new InvalidOperationException("Could not find sample data!");
        }
    }

    #endregion Private Methods

    #region Public Methods

    public static void Main()
    {
        //search sample data
        Console.WriteLine("Search sample folder...");
        var sampleFolder = FindSampleFolder();
        Console.WriteLine($"Use sample folder: {sampleFolder}");

        Console.WriteLine($"Writing results and plots to {Directory.GetCurrentDirectory()}");

        //binaries are not at the emcdx location, so we need to set allowed folders
        DxSettings settings = new() { AllowedFolders = [sampleFolder, sampleFolder + "/../Binary"] };

        //files to work with
        string[] files = ["am_constPK.emcdx", "cw_constPK.emcdx", "pulm1_constPK.emcdx", "pulm2_constPK.emcdx", "pulm2_reverb_constPK.emcdx", "pulm3_constPK.emcdx", "bln_05MHz_constPK.emcdx", "bln_10MHz_constPK.emcdx", "bln_20MHz_constPK.emcdx"];
        foreach (var file in files)
        {
            Console.WriteLine($"Load file: {file}");
            var reader = DxReader.ReadFile(Path.Combine(sampleFolder, file), settings);
            var iq = reader.Read<IQData>().Single();

            //draw some plots
            var plotter = new Plotter(iq, file);
            plotter.PlotTimeDomain();
            plotter.PlotSpectum();

            //write a r&s waveform file
            var rsFileName = RsWaveform.Write(file, iq, reader.File.Description);
            //read the written r&s waveform file
            var rsRoundtripData = RsWaveform.Read(rsFileName);
            //check results
            if (rsRoundtripData.RootMeanSquareOffset != iq.RootMeanSquareOffset) throw new InvalidDataException("RootMeanSquareOffset mismatch!");
            if (rsRoundtripData.PeakOffset != iq.PeakOffset) throw new InvalidDataException("PeakOffset mismatch!");
            if (rsRoundtripData.SampleRate != iq.SampleRate) throw new InvalidDataException("SampleRate mismatch!");
            if (rsRoundtripData.SampleCount != iq.SampleCount) throw new InvalidDataException("SampleCount mismatch!");
            //plot the r&s waveform
            plotter.PlotComparison("RsWaveform", [.. rsRoundtripData.Complex]);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Console.WriteLine("Done");
    }

    #endregion Public Methods
}
