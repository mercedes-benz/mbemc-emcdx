// SPDX-License-Identifier: MIT
namespace MBN50284;

class IndexToFrequency
{
    #region Public Constructors

    public IndexToFrequency(int indexRange, double freqRange)
    {
        FreqRange = freqRange;
        FreqMin = -FreqRange / 2;
        FreqFactor = FreqRange / indexRange;
    }

    #endregion Public Constructors

    #region Public Properties

    public double FreqFactor { get; }
    public double FreqMin { get; }
    public double FreqRange { get; }
    public int IndexRange { get; }

    #endregion Public Properties

    #region Public Methods

    public double GetFrequency(int index) => FreqMin + (index * FreqFactor);

    #endregion Public Methods
}
