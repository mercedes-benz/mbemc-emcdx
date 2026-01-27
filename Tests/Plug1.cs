// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

record Plug1
{
    [DxGenerated(Description = "Zeitpunkt der Messung", Unit = "ms", Name = "tslow")]
    public double TimeStamp { get; set; }

    [DxMapping(Name = "tslow", Description = "Strommessung an Plug1, Pin1", Unit = "A")]
    public double GRA { get; set; }
}
