// SPDX-License-Identifier: MIT
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MBN50284;

public static class OxyPlotExtensions
{
    #region Public Methods

    public static void AddDefaultAxes(this PlotModel model, bool log, string freqUnit)
    {
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;

        if (model.Series.Count == 0) throw new InvalidOperationException("Set line series first!");
        foreach (var series in model.Series.Cast<LineSeries>())
        {
            minX = Math.Min(minX, series.Points.Min(p => p.X));
            maxX = Math.Max(maxX, series.Points.Max(p => p.X));
            minY = Math.Min(minY, series.Points.Min(p => p.Y));
            maxY = Math.Max(maxY, series.Points.Max(p => p.Y));
        }

        model.Axes.Add(new LinearAxis
        {
            Title = $"Frequenz ({freqUnit})",
            Position = AxisPosition.Bottom,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
            Minimum = Math.Floor(minX),
            Maximum = Math.Ceiling(maxX),
        });

        if (log)
        {
            if (minY <= 0) throw new InvalidOperationException("Use LinearAxis with log values!");
            model.Axes.Add(new LogarithmicAxis
            {
                Title = "Amplitude",
                Position = AxisPosition.Left,
                Base = 10,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = minY,
                Maximum = Math.Ceiling(maxY),
            });
        }
        else
        {
            model.Axes.Add(new LinearAxis
            {
                Title = "Amplitude",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = Math.Min(0, Math.Floor(minY)),
                Maximum = Math.Ceiling(maxY),
            });
        }
    }

    public static void AddDefaultSeries(this PlotModel model, IList<DataPoint> points, string title, OxyColor color, double thickness = 1, LineStyle lineStyle = LineStyle.Automatic)
    {
        var series = new LineSeries() { Title = title, Color = color, LineStyle = lineStyle, StrokeThickness = thickness };
        series.Points.AddRange(points);
        model.Series.Add(series);
    }

    #endregion Public Methods
}
