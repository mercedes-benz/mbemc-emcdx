// SPDX-License-Identifier: MIT
using System.Collections;

namespace Mbemc.DataExchange;

/// <summary>Provides a class for generating <see cref="double"/> values based on <see cref="DxValueGeneratorSettings"/>.</summary>
public class DxValueGenerator : IEnumerable<double>
{
    #region Private Classes

    class Enumerator(DxValueGeneratorSettings settings) : IEnumerator<double>
    {
        #region Private Fields

        long step = -1;

        #endregion Private Fields

        #region Public Properties

        public double Current { get; private set; } = double.NaN;
        public DxValueGeneratorSettings Settings { get; } = settings;
        object IEnumerator.Current => Current;

        #endregion Public Properties

        #region Public Methods

        public void Dispose() { }

        public bool MoveNext()
        {
            Current = ++step == 0 ? (double)Settings.Start : Settings.Mode switch
            {
                DxValueGeneratorMode.Linear => (double)(Settings.Start + (Settings.Step * step)),
                DxValueGeneratorMode.Logarithmic => Math.Exp(Math.Log((double)Settings.Start) + (double)(step * Settings.Step)),
                _ => throw new InvalidOperationException($"Invalid Mode {Settings.Mode}!"),
            };
            return step < Settings.ValueCount;
        }

        public void Reset()
        {
            step = -1;
            Current = double.NaN;
        }

        #endregion Public Methods
    }

    #endregion Private Classes

    #region Public Constructors

    /// <summary>Creates a new instance of the <see cref="DxValueGenerator"/> class.</summary>
    /// <param name="settings">Generator settings to use.</param>
    public DxValueGenerator(DxValueGeneratorSettings settings)
    {
        Settings = settings;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>Gets the used generator settings.</summary>
    public DxValueGeneratorSettings Settings { get; }

    #endregion Public Properties

    #region Public Methods

    /// <inheritdoc/>
    public IEnumerator<double> GetEnumerator() => new Enumerator(Settings);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(Settings);

    #endregion Public Methods
}
