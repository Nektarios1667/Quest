﻿namespace Quest.Tools;

public static class StringTools
{
    private static readonly Dictionary<string, string> PluralExceptions = new() {
        { "child", "children" },
        { "person", "people" },
        { "mouse", "mice" },
        { "tooth", "teeth" },
        { "foot", "feet" },
        { "cactus", "cacti" },
        { "fungus", "fungi" },
        { "man", "men" },
        { "woman", "women" },
        { "ox", "oxen" },
        { "goose", "geese" },
        // Unchanged
        { "moose", "moose" },
        { "deer", "deer" },
        { "fish", "fish" },
        { "sheep", "sheep" },
        { "bison", "bison" },
        { "salmon", "salmon" },
        { "trout", "trout" },
    };
    public static string FillCamelSpaces(string str)
    {
        string result = "";
        foreach (char ch in str)
        {
            if (char.IsUpper(ch) && result.Length > 0)
                result += " ";
            result += ch;
        }
        return result;
    }
    public static string ApplyCapitals(string str, string capitals)
    {
        string result = "";
        for (int i = 0; i < str.Length; i++)
        {
            char ch = str[i];
            if (i < capitals.Length && char.IsUpper(capitals[i]))
                result += char.ToUpper(ch);
            else
                result += char.ToLower(ch);
        }
        return result;
    }
    public static string Pluralize(string word)
    {
        // Exceptions
        if (PluralExceptions.TryGetValue(word.ToLower(), out var plural))
            return ApplyCapitals(plural, word);

        // Empty
        if (string.IsNullOrWhiteSpace(word))
            return word;
        string lower = word.ToLower();

        // s, x, z, ch, or sh
        if (lower.EndsWith('s') || lower.EndsWith('x') || lower.EndsWith('z') || lower.EndsWith("ch") || lower.EndsWith("sh"))
            return word + "es";

        // consonant + y
        if (lower.Length >= 2 && !IsVowel(lower[^2]) && lower.EndsWith('y'))
            return word[0..^1] + "ies";

        return word + "s";
    }

    public static bool IsVowel(char c)
    {
        return "aeiou".Contains(c);
    }
}
