// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

record Plug2
{
    [DxGenerated(Name = "t", Unit = "µs", Description = "Zeitpunkt der Messung")]
    public double TimeStamp { get; set; }

    [DxMapping(VariableNames = ["t"], Name = "I_1", Unit = "mA", Description = "Strommessung an Plug2, Pin7, Kl.31, 250 A, PWM=50%")]
    public double Current1 { get; set; }

    [DxMapping(VariableNames = ["t"], Name = "I_2", Unit = "mA", Description = "Strommessung an Plug2, Pin8")]
    public double Current2 { get; set; }

    [DxMapping(VariableNames = ["t"], Unit = "V", Description = "Spannungsmessung an Plug2, Pin8")]
    public double Voltage { get; set; }
}
