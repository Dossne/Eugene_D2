using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Utilities
{
    public static class StringExtensions
    {
        public static string SnakeCaseToPascalCase(this string s)
        {
            const char UNDERSCORE_SYMBOL = '_';
            const int STANDARD_VARIABLE_NAME_MAX_WORDS_COUNT = 5;

            StringBuilder result = new StringBuilder(s, s.Length + STANDARD_VARIABLE_NAME_MAX_WORDS_COUNT);

            for (int i = result.Length - 1; i >= 0; i--)
            {
                if (result[i] == UNDERSCORE_SYMBOL)
                {
                    result[i + 1] = char.ToUpperInvariant(result[i + 1]);
                    result.Remove(i, 1);
                }
            }

            result[0] = char.ToUpperInvariant(result[0]);
            return result.ToString();
        }

        public static string ToSnakeCase(this string str)
        {
            return (Regex.Replace(str, "(?<=[a-z0-9])[A-Z]", "_$0", RegexOptions.Compiled)).ToLowerInvariant();
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static string PrettyCamelCase(this string input)
        {
            return Regex.Replace(input.Replace("_", ""), "((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", " $1").Trim();
        }

        public static string ToTitleCase(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input[0].ToString().ToUpper() + input.Substring(1)
            };

        
        public static string ToColor(this string input, string hexCode)
        {
            return $"<color={hexCode}>{input}</color>";
        }
    }
}