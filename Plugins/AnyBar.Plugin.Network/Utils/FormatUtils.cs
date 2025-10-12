using System.Globalization;

namespace AnyBar.Plugin.Network.Utils;

public static class FormatUtils
{
    private const ulong Kilo = 1024;
    private const ulong Mega = 1024 * Kilo;
    private const ulong Giga = 1024 * Mega;
    private static readonly string BytesFormat = "{0:F0}{1}";

    public static string FormatBytes(float bytes, string unit)
    {
        if (bytes < Kilo)
        {
            return string.Format(CultureInfo.InvariantCulture, BytesFormat, bytes, unit);
        }
        else if (bytes < Mega)
        {
            return string.Format(CultureInfo.InvariantCulture, BytesFormat, bytes / Kilo, $"K{unit}");
        }
        else if (bytes < Giga)
        {
            return string.Format(CultureInfo.InvariantCulture, BytesFormat, bytes / Mega, $"M{unit}");
        }
        else
        {
            return string.Format(CultureInfo.InvariantCulture, BytesFormat, bytes / Giga, $"G{unit}");
        }
    }
}
