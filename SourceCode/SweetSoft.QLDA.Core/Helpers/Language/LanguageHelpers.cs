using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.ResourceTexts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Helpers.Language
{
    /// <summary>
    /// Helper methods for working with supported system languages.
    /// The implementation centralises the language metadata so that
    /// adding a new language only requires registering it in the <see cref="LanguageDefinitions"/> map.
    /// </summary>
    public static class LanguageHelpers
    {
        public const byte English = 1;
        public const byte Vietnamese = 2;

        private const string LanguageCodeCacheKey = "LANGUAGE_CODE_CACHE";
        private const string LanguageTextCacheKey = "LANGUAGE_TEXT_CACHE";
        private const string LanguageNameCacheKey = "LANGUAGE_NAME_CACHE";

        private static readonly IReadOnlyDictionary<byte, LanguageDefinition> LanguageDefinitions;
        private static readonly byte[] AvailableLanguageIds;
        private static readonly LanguageDefinition FallbackLanguage;
        private static readonly Dictionary<string, byte> CultureLookup;

        static LanguageHelpers()
        {
            var languages = new Dictionary<byte, LanguageDefinition>
            {
                [English] = new LanguageDefinition(
                    id: English,
                    cultureName: "en-US",
                    displayName: "English",
                    text: "English",
                    cmsImagePath: "/styles/images/lang-en.jpg",
                    isDefault: false,
                    additionalCultureNames: new[] { "us" }),
                [Vietnamese] = new LanguageDefinition(
                    id: Vietnamese,
                    cultureName: "vi-VN",
                    displayName: "Vietnamese",
                    text: "Vietnamese",
                    cmsImagePath: "/styles/images/lang-vi.jpg",
                    isDefault: true,
                    additionalCultureNames: new[] { "vn" })
            };

            LanguageDefinitions = new ReadOnlyDictionary<byte, LanguageDefinition>(languages);
            AvailableLanguageIds = LanguageDefinitions.Keys.OrderBy(id => id).ToArray();
            FallbackLanguage = LanguageDefinitions.Values.FirstOrDefault(language => language.IsDefault)
                                ?? LanguageDefinitions.Values.First();
            CultureLookup = BuildCultureLookup(LanguageDefinitions.Values);
        }

        /// <summary>
        /// Gets the list of available languages.
        /// The returned array is a copy to prevent accidental external modification.
        /// </summary>
        public static byte[] AvailableLanguages => (byte[])AvailableLanguageIds.Clone();

        /// <summary>
        /// Gets the configured default language or the fallback language when configuration is invalid.
        /// </summary>
        public static byte Defaultlanguage
        {
            get
            {
                var languageFromConfiguration = GetConfiguredDefaultLanguage();
                return languageFromConfiguration?.Id ?? FallbackLanguage.Id;
            }
        }

        /// <summary>
        /// Gets the translated resource text for the supplied language code.
        /// </summary>
        public static string GetResourceText(string languageCode, string messageId)
        {
            return GetResourceTextInternal(CreateCultureFromCode(languageCode), messageId, htmlDecode: true);
        }

        /// <summary>
        /// Gets the translated resource text for the supplied language id.
        /// </summary>
        public static string GetResourceText(byte languageId, string messageId)
        {
            return GetResourceTextInternal(GetLanguageDefinition(languageId).Culture, messageId, htmlDecode: false);
        }

        /// <summary>
        /// Gets the language names displayed to end users.
        /// </summary>
        public static Dictionary<byte, string> LanguageName
        {
            get
            {
                var cacheKey = LanguageNameCacheKey + SweetContext.Current.CurrentLanguageId;
                return GetCachedLanguageDictionary(cacheKey, language => language.DisplayName);
            }
        }

        /// <summary>
        /// Gets the language text values (typically used for UI controls).
        /// </summary>
        public static Dictionary<byte, string> LanguageText
        {
            get
            {
                return GetCachedLanguageDictionary(LanguageTextCacheKey, language => language.Text);
            }
        }

        /// <summary>
        /// Gets the mapping of language id to culture code.
        /// </summary>
        public static Dictionary<byte, string> LanguageCode
        {
            get
            {
                return GetCachedLanguageDictionary(LanguageCodeCacheKey, language => language.Culture.Name);
            }
        }

        /// <summary>
        /// Returns the language id that matches the supplied culture name.
        /// </summary>
        public static byte GetLanguageCodeByCultureName(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return Defaultlanguage;
            }

            if (CultureLookup.TryGetValue(cultureName, out var languageId))
            {
                return languageId;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                var possibleValues = new List<string>
                {
                    culture.Name,
                    culture.TwoLetterISOLanguageName,
                    culture.Parent?.Name
                };

                foreach (var candidate in possibleValues.Where(value => !string.IsNullOrWhiteSpace(value)))
                {
                    if (candidate == null)
                        continue;
                    if (CultureLookup.TryGetValue(candidate, out languageId))
                    {
                        return languageId;
                    }
                }
            }
            catch (CultureNotFoundException)
            {
                // ignored - fall back to the default language below.
            }

            return Defaultlanguage;
        }

        /// <summary>
        /// Gets the culture code for the supplied language id.
        /// </summary>
        public static string GetLanguageCode(byte languageId)
        {
            return GetLanguageDefinition(languageId).Culture.Name;
        }

        /// <summary>
        /// Gets the current language id based on the current UI culture.
        /// </summary>
        public static byte CurrentLanguageId => GetLanguageCodeByCultureName(CultureInfo.CurrentUICulture.Name);

        /// <summary>
        /// Gets the CMS image associated with the given language.
        /// </summary>
        public static string GetCMSLanguageImage(byte languageId)
        {
            return GetLanguageDefinition(languageId).CmsImagePath;
        }

        private static string GetResourceTextInternal(CultureInfo culture, string messageId, bool htmlDecode)
        {
            var text = UITextsReader.GetBackEndResourceText(culture, messageId);
            return htmlDecode ? HttpUtility.HtmlDecode(text) : text;
        }

        private static CultureInfo CreateCultureFromCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return GetLanguageDefinition(Defaultlanguage).Culture;
            }

            return CultureInfo.GetCultureInfo(languageCode);
        }

        private static LanguageDefinition GetLanguageDefinition(byte languageId)
        {
            if (LanguageDefinitions.TryGetValue(languageId, out var definition))
            {
                return definition;
            }

            return FallbackLanguage;
        }

        private static Dictionary<byte, string> GetCachedLanguageDictionary(string cacheKey, Func<LanguageDefinition, string> selector)
        {
            var cached = AppCache.Get(cacheKey) as Dictionary<byte, string>;
            if (cached != null && cached.Count == LanguageDefinitions.Count)
            {
                return cached;
            }

            var dictionary = LanguageDefinitions.Values.ToDictionary(language => language.Id, selector);
            AppCache.Max(cacheKey, dictionary);
            return dictionary;
        }

        private static LanguageDefinition GetConfiguredDefaultLanguage()
        {
            var configurationValue = ConfigurationManager.AppSettings["DefaultLanguage"];
            if (string.IsNullOrWhiteSpace(configurationValue))
            {
                return null;
            }

            if (byte.TryParse(configurationValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var languageId)
                && LanguageDefinitions.TryGetValue(languageId, out var definition))
            {
                return definition;
            }

            return null;
        }

        private static Dictionary<string, byte> BuildCultureLookup(IEnumerable<LanguageDefinition> languages)
        {
            var lookup = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

            foreach (var language in languages)
            {
                foreach (var cultureKey in language.CultureKeys)
                {
                    if (!lookup.ContainsKey(cultureKey))
                    {
                        lookup[cultureKey] = language.Id;
                    }
                }
            }

            return lookup;
        }


    }
}
