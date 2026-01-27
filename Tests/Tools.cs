// SPDX-License-Identifier: MIT
namespace Mbemc.DataExchange.Tests;

class Tools
{
    #region Internal Methods

    internal static string CreateTemp()
    {
        var folder = Path.GetTempPath();
        for (var i = 0; ; i++)
        {
            if (i > 20) throw new InvalidOperationException("Could not create empty temp directory!");
            var temp = Path.Combine(folder, $"_emcdx_test_{Guid.NewGuid()}");
            if (Directory.Exists(temp)) continue;
            Directory.CreateDirectory(temp);
            return temp;
        }
    }

    #endregion Internal Methods
}
