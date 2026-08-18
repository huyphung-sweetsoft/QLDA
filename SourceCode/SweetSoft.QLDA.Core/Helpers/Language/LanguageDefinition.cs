using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Language
{
    internal sealed class LanguageDefinition
    {
        public LanguageDefinition(
            byte id,
            string cultureName,
            string displayName,
            string text,
            string cmsImagePath,
            bool isDefault,
            IEnumerable<string> additionalCultureNames)
        {
            Id = id;
            Culture = CultureInfo.GetCultureInfo(cultureName);
            DisplayName = displayName;
            Text = text;
            CmsImagePath = cmsImagePath;
            IsDefault = isDefault;

            var cultureKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Culture.Name,
                    Culture.TwoLetterISOLanguageName,
                    cultureName
                };

            if (!string.IsNullOrWhiteSpace(Culture.Parent?.Name))
            {
                cultureKeys.Add(Culture.Parent.Name);
            }

            if (additionalCultureNames != null)
            {
                foreach (var alias in additionalCultureNames)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        cultureKeys.Add(alias);
                    }
                }
            }

            CultureKeys = new ReadOnlyCollection<string>(cultureKeys.ToList());
        }

        public byte Id { get; }

        public CultureInfo Culture { get; }

        public string DisplayName { get; }

        public string Text { get; }

        public string CmsImagePath { get; }

        public bool IsDefault { get; }

        public IReadOnlyCollection<string> CultureKeys { get; }
    }
}
