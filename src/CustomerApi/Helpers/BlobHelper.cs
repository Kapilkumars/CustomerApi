using System.Globalization;
using System.Text;

namespace CustomerCustomerApi.Helpers;

public static class BlobHelper
{
    private static readonly char[] RestrictedCharacters = { '(', ')', '=', ';', ',', '/', '#', '@', '%', '*', '+', '\t', '\n', '\r' };
    public static string? SanitizeTagName(string? tagName)
    {
        if (string.IsNullOrEmpty(tagName)) return null;
        foreach (var restrictedChar in RestrictedCharacters)
            tagName = tagName.Replace(restrictedChar.ToString(), string.Empty);
        return tagName.Trim();
    }
}