// SPDX-License-Identifier: MIT
using System.Numerics;
using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.SkiaSharp;

namespace MBN50284;

class Plotter
{
    #region Private Methods

    double CoherentGain(IList<double> window)
    {
        var sum = window.Sum();
        var sumSquared = window.Sum(x => x * x);
        return (sum * sum) / sumSquared / window.Count;
    }

    void Extend(ref Complex[] signal, int lenMin)
    {
        var lenOld = signal.Length;
        //new length has to be a multiple of the original signal length
        var lenNew = lenMin % lenOld == 0 ? lenMin : ((lenMin / lenOld) + 1) * lenOld;
        Array.Resize(ref signal, lenNew);
        for (var i = lenOld; i < lenNew; i += lenOld)
        {
            Array.Copy(signal, 0, signal, i, lenOld);
        }
    }

    void Plot(string title, Action<PlotModel>? modelAction = null)
    {
        var fullTitle = $"{Name} {title}";
        Console.WriteLine($"... plotting {fullTitle}");
        var model = new PlotModel { Title = fullTitle, Background = OxyColors.White };
        modelAction?.Invoke(model);
        if (model.Legends.Count == 0)
        {
            model.Legends.Add(new Legend() { LegendTitle = "Legende", LegendPosition = LegendPosition.RightTop, });
        }
        PngExporter.Export(model, $"{fullTitle}.png", 3840, 2160);
    }

    void PlotCrestFactor(Complex[] spectrum)
    {
        var crest = new List<DataPoint>();
        var half = spectrum.Length / 2;
        var freqFactor = IQ.SampleRate / spectrum.Length;
        var _1khz = (int)(1000 / freqFactor);
        var step = Math.Max(1, _1khz);
        var lastFreq = _1khz * freqFactor;
        for (var i = 0; i < spectrum.Length; i += step)
        {
            var freq = IndexToFrequency.GetFrequency(i - half) / 1_000_000d;
            var avg = 0d;
            var max = double.MinValue;
            for (var n = 0; n < _1khz; n++)
            {
                avg += spectrum[n].Magnitude;
                max = Math.Max(max, spectrum[n].Magnitude);
            }
            avg /= _1khz;

            crest.Add(new DataPoint(freq, ToDecibel(max / avg)));
        }
        Plot("Crest", m =>
        {
            m.AddDefaultSeries(crest, "Crest", OxyColors.Blue);
            m.AddDefaultAxes(false, "MHz");
        });
    }

    void PlotSpectrum(string title, Complex[] spectrum)
    {
        var points = spectrum.Select((v, i) => new DataPoint(IndexToFrequency.GetFrequency(i) / 1_000_000d, v.Magnitude)).ToList();
        Plot(title, m =>
        {
            m.AddDefaultSeries(points, "Spectrum", OxyColors.Red);
            m.AddDefaultAxes(true, "MHz");
        });
    }

    void PlotWindowed(Complex[] spectrum, double bandwidth)
    {
        //TODO: implement 6dB windowing in time domain with coherent gain normalizing

        double[] window;
        var windowSize = (int)(IQ.SampleRate / bandwidth);

        //we are doing to shortcuts here: take the default blackman harris window in frequency domain with 120 kHz
        {
            var windowFD = Window.BlackmanHarris(windowSize);
            var correction = windowFD.Max();
            window = [.. windowFD.Select(v => v / correction)];
        }

        /* plot the used window
        Plot("Window", m =>
        {
            var idx = new IndexToFrequency(window.Length, bandwidth);
            var points = window.Select((v, i) => new DataPoint(idx.GetFrequency(i) / 1000d, v)).ToList();
            m.AddDefaultSeries(points, "Value", OxyColors.Blue);
            m.AddDefaultAxes(true, "kHz");
            m.Legends.Add(new Legend() { LegendTitle = "Legende", LegendPosition = LegendPosition.RightTop, });
        });
        */

        //last possible window start index
        var maxIndex = spectrum.Length - window.Length;

        //we want 2000 points if possible, but at least /2 bandwidth overlaps
        var increment = Math.Max(1, maxIndex / 2000);
        var incrementByBandwidth = 2 * maxIndex / windowSize;
        if (incrementByBandwidth > 0) increment = Math.Min(increment, incrementByBandwidth);

        //prepare results
        var pointsMax = new List<DataPoint>(spectrum.Length / increment);
        var pointsAvg = new List<DataPoint>(spectrum.Length / increment);

        for (var index = 0; index < maxIndex;)
        {
            //apply window and calc avg and max
            var max = double.MinValue;
            var avg = 0d;
            for (var n = 0; n < windowSize; n++)
            {
                var magnitude = spectrum[index + n].Magnitude;
                magnitude *= window[n];
                avg += magnitude;
                max = Math.Max(max, magnitude);
            }
            avg /= windowSize;
            //set freq to center of window
            var freq = IndexToFrequency.GetFrequency(index + (windowSize / 2)) / 1_000_000d;
            //save points
            pointsAvg.Add(new DataPoint(freq, avg));
            pointsMax.Add(new DataPoint(freq, max));

            index += increment;
        }

        Plot($"Spectrum Bandwidth {bandwidth / 1000} kHz", m =>
        {
            m.AddDefaultSeries(pointsMax, "Maximum", OxyColors.Blue);
            m.AddDefaultSeries(pointsAvg, "Average", OxyColors.Red);
            m.AddDefaultAxes(true, "MHz");
        });

        Plot($"Spectrum Bandwidth {bandwidth / 1000} kHz in dB", m =>
        {
            m.AddDefaultSeries([.. pointsMax.Select(ToDecibel)], "Maximum", OxyColors.Blue);
            m.AddDefaultSeries([.. pointsAvg.Select(ToDecibel)], "Average", OxyColors.Red);
            m.AddDefaultAxes(false, "MHz");
        });
    }

    double ToDecibel(double value) => 20 * Math.Log10(value);

    DataPoint ToDecibel(DataPoint value) => new(value.X, 20 * Math.Log10(1_000_000d * value.Y));

    #endregion Private Methods

    #region Public Constructors

    public Plotter(IQData iq, string sourceFileName)
    {
        IQ = iq;
        SourceFileName = sourceFileName;
        Name = Path.GetFileNameWithoutExtension(SourceFileName);
        IndexToFrequency = new IndexToFrequency(IQ.SampleCount, IQ.SampleRate);
    }

    #endregion Public Constructors

    #region Public Properties

    public IndexToFrequency IndexToFrequency { get; }
    public IQData IQ { get; }
    public string Name { get; }
    public string SourceFileName { get; }

    #endregion Public Properties

    #region Public Methods

    public void PlotComparison(string name, Complex[] roundtripValues)
    {
        var signal = IQ.Complex.ToArray();
        if (SourceFileName.Contains("bln"))
        {
            //cut 100 us
            signal = [.. signal.Take((int)(IQ.SampleRate * 100d / 1_000_000d))];
            roundtripValues = [.. roundtripValues.Take((int)(IQ.SampleRate * 100d / 1_000_000d))];
        }
        else
        {
            //10ms
            Extend(ref signal, (int)(IQ.SampleRate / 100d));
            Extend(ref roundtripValues, (int)(IQ.SampleRate / 100d));
        }

        {
            var real1 = signal.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Real)).ToList();
            var real2 = roundtripValues.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Real)).ToList();
            Plot($"Real+{name}", m =>
            {
                m.Axes.Add(new LinearAxis { Title = "Zeit (ms)", Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, });
                m.Axes.Add(new LinearAxis { Title = "Amplitude", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Minimum = -1, Maximum = 1 });

                m.AddDefaultSeries(real1, (IQ.Name ?? Name) + " Real", OxyColors.Green, 5, LineStyle.Dot);
                m.AddDefaultSeries(real2, name + " Real", OxyColors.Red, 2, LineStyle.DashDot);
            });
        }
        {
            var imag1 = signal.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Imaginary)).ToList();
            var imag2 = roundtripValues.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Imaginary)).ToList();
            Plot($"Imaginary+{name}", m =>
            {
                m.Axes.Add(new LinearAxis { Title = "Zeit (ms)", Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, });
                m.Axes.Add(new LinearAxis { Title = "Amplitude", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Minimum = -1, Maximum = 1 });

                m.AddDefaultSeries(imag1, (IQ.Name ?? Name) + " Imaginary", OxyColors.Green, 5, LineStyle.Dot);
                m.AddDefaultSeries(imag2, name + " Imaginary", OxyColors.Red, 2, LineStyle.DashDot);
            });
        }
    }

    public void PlotSpectum()
    {
        var signal = IQ.Complex.ToArray();

        Console.WriteLine("... Calculate FFT");
        var fft = new FFT(signal);

        /* plot full spectrum
        Console.WriteLine($"... Plot Spectrum");
        PlotSpectrum("Spectrum", fft.Spectrum);
        */

        fft.Shift();

        /* plot full spectrum shifted
        Console.WriteLine($"... Plot Spectrum Shifted");
        PlotSpectrum("Spectrum Shifted", fft.Spectrum);
        */

        if (SourceFileName.Contains("bln"))
        {
            Console.WriteLine($"... Plot Crest Factor");
            PlotCrestFactor(fft.Spectrum);
        }

        const double bandwidth = 120_000d;
        Console.WriteLine($"... Plot Spectrum with bandwidth = {bandwidth}");
        PlotWindowed(fft.Spectrum, bandwidth);
    }

    public void PlotTimeDomain()
    {
        var signal = IQ.Complex.ToArray();
        if (SourceFileName.Contains("bln"))
        {
            //cut 100 us
            signal = [.. signal.Take((int)(IQ.SampleRate * 100d / 1_000_000d))];
        }
        else
        {
            //10ms
            Extend(ref signal, (int)(IQ.SampleRate / 100d));
        }

        var magitude = signal.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Magnitude)).ToList();
        var phase = signal.Select((c, i) => new DataPoint(1000d * i / IQ.SampleRate, c.Phase)).ToList();
        Plot("Magnitude+Phase", m =>
        {
            m.Axes.Add(new LinearAxis { Title = "Zeit (ms)", Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot });
            m.Axes.Add(new LinearAxis { Title = "Amplitude", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, });
            m.AddDefaultSeries(phase, (IQ.Name ?? Name) + " Phase", OxyColors.Green);
            m.AddDefaultSeries(magitude, (IQ.Name ?? Name) + " Magnitude", OxyColors.Blue);
        });
    }

    #endregion Public Methods
}
