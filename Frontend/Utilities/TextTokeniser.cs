using System.Collections.Generic;

namespace ClearText.Utilities;

public static class TextTokeniser
{
    /// <summary>
    /// Splits text into whitespace-delimited tokens
    /// and returns (Text, StartOffset) pairs.
    /// </summary>
    internal static List<(string Text, int Start)> TokeniseOnWhitespace(string text)
    {
        var tokens = new List<(string, int)>();

        var i = 0;
        while (i < text.Length)
        {
            // Skip whitespace (handles single or multiple spaces, tabs, newlines)
            if (char.IsWhiteSpace(text[i]))
            {
                i++;
                continue;
            }

            var start = i;

            // Ignore characters until next whitespace
            while (i < text.Length && !char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            tokens.Add((text[start..i], start));
        }

        return tokens;
    }
}