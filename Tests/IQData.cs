// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;
record IQData : IDxName, IDxDescription
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    [DxVariable(Name = "PK2RMS", Unit = "dB", Description = "Ratio of PK to RMS values of the IQ data.")]
    public double PeakToRootMeanSquare { get; init; }

    [DxVariable(Name = "PK2AV", Unit = "dB", Description = "Ratio of PK to AV values of the IQ data.")]
    public double PeakToAverage { get; init; }

    [DxVariable(Name = "Clock", Unit = "Hz", Description = "Clock or sampling rate of the IQ data.")]
    public double SampleRate { get; init; }

    [DxVariable(Name = "RMS_OFFS", Unit = "dB", Description = "For Rohde&Schwarz vector signal generators only! Recommended to be given in wv-file as 1st number in tag 'LEVEL_OFFS'.")]
    public double RootMeanSquareOffset { get; init; }

    [DxVariable(Name = "PK_OFFS", Unit = "dB", Description = "For Rohde&Schwarz vector signal generators only! Recommended to be given in wv-file as 2nd number in tag 'LEVEL_OFFS'.")]
    public double PeakOffset { get; init; }

    [DxVariable(Name = "Real_Part", Unit = "FS", Description = "Real part of IQ data on a scale FS (full scale = +1.0). Note: PK(sqrt(I^2+Q^2)) may be <1.0, see PK_OFFS.")]
    public IList<double> Real { get; init; } = [];

    [DxVariable(Name = "Imag_Part", Unit = "FS", Description = "Imaginary part of IQ data on a scale FS (full scale = +1.0). Note: PK(sqrt(I^2+Q^2)) may be <1.0, see PK_OFFS.")]
    public IList<double> Imaginary { get; init; } = [];
}
