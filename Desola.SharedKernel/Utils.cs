namespace Desola.Common;

public class Utils
{
    public static List<string> ParseCommaSeparatedList(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? new List<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static int? ParseIntParameter(string? value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }

    public static bool? ParseBoolParameter(string? value)
    {
        return bool.TryParse(value, out var result) ? result : null;
    }

}