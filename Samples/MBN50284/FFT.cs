// SPDX-License-Identifier: MIT
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace MBN50284;

class FFT
{
    #region Public Constructors

    public FFT(IList<double> signal) : this(signal.Select(s => new Complex(s, 0)).ToArray())
    {
    }

    public FFT(IList<Complex> signal)
    {
        SignalLength = (uint)signal.Count;
        var N = SignalLength;
        var spectrum = new Complex[N];
        signal.CopyTo(spectrum, 0);
        Fourier.Forward(spectrum, FourierOptions.Matlab);

        //normalize
        var factor = 1d / spectrum.Length;
        for (var i = 0; i < spectrum.Length; i++) spectrum[i] *= factor;

        var maxSignal = signal.Max(s => s.Magnitude);
        var maxSpectrum = spectrum.Max(s => s.Magnitude);

        Spectrum = spectrum;
    }

    #endregion Public Constructors

    #region Public Properties

    public bool IsShifted { get; private set; }
    public uint SignalLength { get; private set; }
    public Complex[] Spectrum { get; }

    #endregion Public Properties

    #region Public Methods

    public void Shift()
    {
        var N = Spectrum.Length;
        var halfN = N / 2;
        for (var i = 0; i < halfN; i++)
        {
            //swap values via tuple
            (Spectrum[i], Spectrum[i + halfN]) = (Spectrum[i + halfN], Spectrum[i]);
        }
        IsShifted = !IsShifted;
    }

    #endregion Public Methods
}
