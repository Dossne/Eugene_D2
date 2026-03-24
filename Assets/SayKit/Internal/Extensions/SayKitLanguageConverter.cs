using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public static class SayKitLanguageConverter
    {
        private static readonly Dictionary<string, SayKitLanguage> _languageMap =
            new Dictionary<string, SayKitLanguage>(StringComparer.OrdinalIgnoreCase)
            {
                { "af", SayKitLanguage.Afrikaans },
                { "ar", SayKitLanguage.Arabic },
                { "eu", SayKitLanguage.Basque },
                { "be", SayKitLanguage.Belarusian },
                { "bg", SayKitLanguage.Bulgarian },
                { "ca", SayKitLanguage.Catalan },
                { "zh", SayKitLanguage.Chinese },
                { "cs", SayKitLanguage.Czech },
                { "da", SayKitLanguage.Danish },
                { "nl", SayKitLanguage.Dutch },
                { "en", SayKitLanguage.English },
                { "et", SayKitLanguage.Estonian },
                { "fo", SayKitLanguage.Faroese },
                { "fi", SayKitLanguage.Finnish },
                { "fr", SayKitLanguage.French },
                { "de", SayKitLanguage.German },
                { "el", SayKitLanguage.Greek },
                { "he", SayKitLanguage.Hebrew },
                { "hu", SayKitLanguage.Hungarian },
                { "is", SayKitLanguage.Icelandic },
                { "id", SayKitLanguage.Indonesian },
                { "it", SayKitLanguage.Italian },
                { "ja", SayKitLanguage.Japanese },
                { "ko", SayKitLanguage.Korean },
                { "lv", SayKitLanguage.Latvian },
                { "lt", SayKitLanguage.Lithuanian },
                { "no", SayKitLanguage.Norwegian },
                { "pl", SayKitLanguage.Polish },
                { "pt", SayKitLanguage.Portuguese },
                { "ro", SayKitLanguage.Romanian },
                { "ru", SayKitLanguage.Russian },
                { "hr", SayKitLanguage.SerboCroatian },
                { "sk", SayKitLanguage.Slovak },
                { "sl", SayKitLanguage.Slovenian },
                { "es", SayKitLanguage.Spanish },
                { "sv", SayKitLanguage.Swedish },
                { "th", SayKitLanguage.Thai },
                { "tr", SayKitLanguage.Turkish },
                { "uk", SayKitLanguage.Ukrainian },
                { "vi", SayKitLanguage.Vietnamese },
                { "zh-Hans", SayKitLanguage.ChineseSimplified },
                { "zh-Hant", SayKitLanguage.ChineseTraditional },
                { "hi", SayKitLanguage.Hindi },
            };

        private static readonly Dictionary<SayKitLanguage, string> _languageReverseMap = new Dictionary<SayKitLanguage, string>()
        {
            { SayKitLanguage.Afrikaans, "af" },
            { SayKitLanguage.Arabic, "ar" },
            { SayKitLanguage.Basque, "eu" },
            { SayKitLanguage.Belarusian, "be" },
            { SayKitLanguage.Bulgarian, "bg" },
            { SayKitLanguage.Catalan, "ca" },
            { SayKitLanguage.Chinese, "zh" },
            { SayKitLanguage.Czech, "cs" },
            { SayKitLanguage.Danish, "da" },
            { SayKitLanguage.Dutch, "nl" },
            { SayKitLanguage.English, "en" },
            { SayKitLanguage.Estonian, "et" },
            { SayKitLanguage.Faroese, "fo" },
            { SayKitLanguage.Finnish, "fi" },
            { SayKitLanguage.French, "fr" },
            { SayKitLanguage.German, "de" },
            { SayKitLanguage.Greek, "el" },
            { SayKitLanguage.Hebrew, "he" },
            { SayKitLanguage.Hungarian, "hu" },
            { SayKitLanguage.Icelandic, "is" },
            { SayKitLanguage.Indonesian, "id" },
            { SayKitLanguage.Italian, "it" },
            { SayKitLanguage.Japanese, "ja" },
            { SayKitLanguage.Korean, "ko" },
            { SayKitLanguage.Latvian, "lv" },
            { SayKitLanguage.Lithuanian, "lt" },
            { SayKitLanguage.Norwegian, "no" },
            { SayKitLanguage.Polish, "pl" },
            { SayKitLanguage.Portuguese, "pt" },
            { SayKitLanguage.Romanian, "ro" },
            { SayKitLanguage.Russian, "ru" },
            { SayKitLanguage.SerboCroatian, "hr" },
            { SayKitLanguage.Slovak, "sk" },
            { SayKitLanguage.Slovenian, "sl" },
            { SayKitLanguage.Spanish, "es" },
            { SayKitLanguage.Swedish, "sv" },
            { SayKitLanguage.Thai, "th" },
            { SayKitLanguage.Turkish, "tr" },
            { SayKitLanguage.Ukrainian, "uk" },
            { SayKitLanguage.Vietnamese, "vi" },
            { SayKitLanguage.ChineseSimplified, "zh-Hans" },
            { SayKitLanguage.ChineseTraditional, "zh-Hant" },
            { SayKitLanguage.Hindi, "hi" },
        };

        public static SayKitLanguage ConvertToSayKitLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return SayKitLanguage.Unknown;
            }

            var normalizedIso = ConvertToIETF(code);

            if (_languageMap.TryGetValue(normalizedIso, out var fullMatch))
            {
                return fullMatch;
            }
            
            var baseCode = normalizedIso.Split('-')[0];

            if (_languageMap.TryGetValue(baseCode, out var baseMatch))
            {
                return baseMatch;
            }

            return SayKitLanguage.English;
        }

        public static string ConvertFromSayKitLanguage(SayKitLanguage language)
        {
            if (language == SayKitLanguage.ChineseSimplified || language == SayKitLanguage.ChineseTraditional)
            {
                if (_languageReverseMap.TryGetValue(language, out var lngCode))
                {
                    return ConvertFromIETF(lngCode);
                }
            }
            
            if (_languageReverseMap.TryGetValue(language, out var code))
            {
                return code;
            }
            
            return "en";
        }
        
        public static string ConvertToIETF(string langCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(langCode))
                {
                    SayKitDebug.LogWarning("[ConvertToIETFFormat] Input language code is null or whitespace.");
                    return langCode;
                }

                var trimmed = langCode.Trim();
                var parts = trimmed.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                {
                    SayKitDebug.LogWarning($"[ConvertToIETFFormat] Failed to split language code: \"{langCode}\"");
                    return langCode;
                }

                var result = parts[0].ToLowerInvariant();

                foreach (var partRaw in parts.Skip(1))
                {
                    var part = partRaw.Trim();

                    if (part.Length == 4 && part.All(char.IsLetter))
                    {
                        var script = char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant();
                        result += "-" + script;
                    }
                    else if ((part.Length == 2 && part.All(char.IsLetter)) ||
                             (part.Length == 3 && part.All(char.IsDigit)))
                    {
                        result += "-" + part.ToUpperInvariant();
                    }
                    else
                    {
                        result += "-" + part.ToLowerInvariant();
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[ConvertToIETFFormat] Exception occurred for language code \"{langCode}\": {e}");
                return langCode;
            }
        }
        
        public static string ConvertFromIETF(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    SayKitDebug.LogWarning("[ConvertFromIETF] Input language code is null or whitespace.");
                    return code;
                }

                var trimmed = code.Trim();
                var parts = trimmed.Split('-');

                var result = string.Join("_", parts.Select(p => p.ToLowerInvariant()));
                return result;
            }
            catch (Exception e)
            {
                SayKitDebug.LogError($"[ConvertFromIETF] Exception occurred for language code \"{code}\": {e}");
                return code;
            }
        }

        public static CultureInfo GetCultureInfoFromCode(string code)
        {
            var codeIETF = ConvertToIETF(code);
            var cultureInfo = CultureInfo.InvariantCulture;

            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(codeIETF);
                return cultureInfo;
            }
            catch (CultureNotFoundException)
            {
                var parts = codeIETF.Split('-');
                if (parts.Length > 1)
                {
                    var langOnly = parts[0];

                    try
                    {
                        return CultureInfo.GetCultureInfo(langOnly);
                    }
                    catch (CultureNotFoundException)
                    {
                        SayKitDebug.LogWarning($"Neither '{codeIETF}' nor '{langOnly}' are valid cultures.");
                        return cultureInfo;
                    }
                }

                return cultureInfo;
            }
        }
    }
}