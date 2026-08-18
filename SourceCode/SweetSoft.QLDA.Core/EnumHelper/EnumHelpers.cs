using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SweetSoft.QLDA.Core.EnumHelper
{
    public static class EnumHelpers
    {
        public enum BadgeStyle
        {
            Success,
            Warning,
            Error,
            Info,
            Secondary,
            Primary,
            Light,
            Dark
        }

        #region Core Attribute Retrieval Methods

        /// <summary>
        /// Lấy attribute từ enum value
        /// </summary>
        public static TAttribute GetEnumAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            return field?.GetCustomAttribute<TAttribute>();
        }

        /// <summary>
        /// Lấy tất cả attributes của một loại từ enum value
        /// </summary>
        public static TAttribute[] GetEnumAttributes<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            return field?.GetCustomAttributes<TAttribute>().ToArray() ?? new TAttribute[0];
        }

        #endregion

        #region Display Text Methods

        /// <summary>
        /// Lấy text hiển thị từ ERenderAttribute hoặc Description
        /// </summary>
        public static string GetDisplayText(this Enum enumValue)
        {
            // Ưu tiên ERenderAttribute
            var renderAttr = enumValue.GetEnumAttribute<ERenderAttribute>();
            if (renderAttr != null)
                return renderAttr.DisplayName;

            // Fallback sang DescriptionAttribute
            var descAttr = enumValue.GetEnumAttribute<DescriptionAttribute>();
            if (descAttr != null)
                return descAttr.Description;

            // Cuối cùng là tên enum
            return enumValue.ToString();
        }

        /// <summary>
        /// Lấy text hiển thị từ string value (an toàn)
        /// </summary>
        public static string GetDisplayTextSafe<TEnum>(string enumValue, string defaultText = "Không xác định")
            where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrEmpty(enumValue))
                    return defaultText;

                if (Enum.TryParse<TEnum>(enumValue, true, out var result))
                    return result.GetDisplayText();

                return defaultText;
            }
            catch
            {
                return defaultText;
            }
        }

        /// <summary>
        /// Lấy tên enum
        /// </summary>
        public static string GetNameSafe<TEnum>(string enumValue)
            where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrEmpty(enumValue))
                    return "";

                if (Enum.TryParse<TEnum>(enumValue, true, out var result))
                    return result.ToString();

                return "";
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Color Methods

        /// <summary>
        /// Lấy màu từ EColorAttribute
        /// </summary>
        public static string GetColor(this Enum enumValue)
        {
            var colorAttr = enumValue.GetEnumAttribute<EColorAttribute>();
            return colorAttr?.Color;
        }

        /// <summary>
        /// Lấy màu từ string value (an toàn)
        /// </summary>
        public static string GetColorSafe<TEnum>(string enumValue, string defaultColor = "#6c757d")
            where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrEmpty(enumValue))
                    return defaultColor;

                if (Enum.TryParse<TEnum>(enumValue, true, out var result))
                    return result.GetColor() ?? defaultColor;

                return defaultColor;
            }
            catch
            {
                return defaultColor;
            }
        }

        #endregion

        #region HTML Generation Methods

        /// <summary>
        /// Tạo HTML span với text và màu từ enum value
        /// </summary>
        public static string ToHtmlSpan(this Enum enumValue, string cssClass = "", string style = "")
        {
            var displayText = enumValue.GetDisplayText();
            var color = enumValue.GetColor();

            var styleAttr = string.Empty;
            if (!string.IsNullOrEmpty(color))
            {
                styleAttr = $"color: {color};";
            }

            if (!string.IsNullOrEmpty(style))
            {
                styleAttr += style;
            }

            var classAttr = string.IsNullOrEmpty(cssClass) ? "" : $" class='{cssClass}'";
            var styleAttribute = string.IsNullOrEmpty(styleAttr) ? "" : $" style='{styleAttr}'";

            return $"<span{classAttr}{styleAttribute}>{displayText}</span>";
        }

        /// <summary>
        /// Tạo HTML badge với text và màu từ enum value
        /// </summary>
        public static string ToHtmlBadge(this Enum enumValue, BadgeStyle badgeStyle = BadgeStyle.Secondary)
        {
            var displayText = enumValue.GetDisplayText();
            var color = enumValue.GetColor();
            var badgeClass = GetBadgeClass(badgeStyle);

            var styleAttr = string.Empty;
            if (!string.IsNullOrEmpty(color))
            {
                styleAttr = $" style='background-color: {color} !important; border-color: {color} !important;'";
            }

            return $"<span class='{badgeClass}'{styleAttr}>{displayText}</span>";
        }

        /// <summary>
        /// Tạo HTML span với text và màu từ string value (an toàn)
        /// </summary>
        public static string ToHtmlSpanSafe<TEnum>(string enumValue, string cssClass = "",
            string style = "", string defaultText = "Không xác định", string defaultColor = "#6c757d")
            where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrEmpty(enumValue))
                    return CreateHtmlSpan(defaultText, defaultColor, cssClass, style);

                if (Enum.TryParse<TEnum>(enumValue, true, out var result))
                    return result.ToHtmlSpan(cssClass, style);

                return CreateHtmlSpan(defaultText, defaultColor, cssClass, style);
            }
            catch
            {
                return CreateHtmlSpan(defaultText, defaultColor, cssClass, style);
            }
        }

        /// <summary>
        /// Tạo HTML badge với text và màu từ string value (an toàn)
        /// </summary>
        public static string ToHtmlBadgeSafe<TEnum>(string enumValue, BadgeStyle badgeStyle = BadgeStyle.Secondary,
            string defaultText = "Không xác định", string defaultColor = "#6c757d")
            where TEnum : struct, Enum
        {
            try
            {
                if (string.IsNullOrEmpty(enumValue))
                    return CreateHtmlBadge(defaultText, defaultColor, badgeStyle);

                if (Enum.TryParse<TEnum>(enumValue, true, out var result))
                    return result.ToHtmlBadge(badgeStyle);

                return CreateHtmlBadge(defaultText, defaultColor, badgeStyle);
            }
            catch
            {
                return CreateHtmlBadge(defaultText, defaultColor, badgeStyle);
            }
        }

        #endregion

        #region Helper Methods

        private static string CreateHtmlSpan(string text, string color, string cssClass, string style)
        {
            var styleAttr = $"color: {color};";
            if (!string.IsNullOrEmpty(style))
                styleAttr += style;

            var classAttr = string.IsNullOrEmpty(cssClass) ? "" : $" class='{cssClass}'";
            return $"<span{classAttr} style='{styleAttr}'>{text}</span>";
        }

        private static string CreateHtmlBadge(string text, string color, BadgeStyle badgeStyle)
        {
            var badgeClass = GetBadgeClass(badgeStyle);
            var styleAttr = $" style='background-color: {color} !important; border-color: {color} !important;'";
            return $"<span class='{badgeClass}'{styleAttr}>{text}</span>";
        }

        private static string GetBadgeClass(BadgeStyle style)
        {
            switch (style)
            {
                case BadgeStyle.Success:
                    return "badge rounded-pill bg-success";
                case BadgeStyle.Warning:
                    return "badge rounded-pill bg-warning";
                case BadgeStyle.Error:
                    return "badge rounded-pill bg-danger";
                case BadgeStyle.Info:
                    return "badge rounded-pill bg-info";
                case BadgeStyle.Primary:
                    return "badge rounded-pill bg-primary";
                case BadgeStyle.Secondary:
                    return "badge rounded-pill bg-secondary";
                case BadgeStyle.Light:
                    return "badge rounded-pill bg-light text-dark";
                case BadgeStyle.Dark:
                    return "badge rounded-pill bg-dark";
                default:
                    return "badge rounded-pill bg-secondary";
            }
        }

        #endregion

        #region Legacy Support (Backward Compatibility)

        [Obsolete("Use GetDisplayText() instead")]
        public static string GetRenderValue(this Enum value) => value.GetDisplayText();

        [Obsolete("Use GetDisplayTextSafe<T>() instead")]
        public static string GetRenderFromDbValueSafe<TEnum>(string dbValue, string defaultRender = "Unknown")
            where TEnum : struct, Enum => GetDisplayTextSafe<TEnum>(dbValue, defaultRender);

        [Obsolete("Use ToHtmlBadgeSafe<T>() instead")]
        public static string GetRenderHtmlFromDbValueSafe<TEnum>(string dbValue, BadgeStyle badgeStyle,
            string defaultRender = "Unknown") where TEnum : struct, Enum
            => ToHtmlBadgeSafe<TEnum>(dbValue, badgeStyle, defaultRender);
        public static string GetERenderText(Type enumType, object rawValue)
        {
            if (!enumType.IsEnum || rawValue == null)
                return rawValue?.ToString() ?? string.Empty;

            try
            {
                var enumName = rawValue.ToString();
                if (!Enum.IsDefined(enumType, enumName))
                    return enumName;

                var enumVal = (Enum)Enum.Parse(enumType, enumName);
                return GetERenderText(enumVal);
            }
            catch
            {
                return rawValue.ToString();
            }
        }
        private static string GetERenderText(Enum value)
        {
            var type = value.GetType();
            var memInfo = type.GetMember(value.ToString());
            var attr = memInfo[0].GetCustomAttributes(typeof(ERenderAttribute), false)
                                .FirstOrDefault() as ERenderAttribute;

            return attr?.DisplayName ?? value.ToString();
        }
        #endregion
    }
}
