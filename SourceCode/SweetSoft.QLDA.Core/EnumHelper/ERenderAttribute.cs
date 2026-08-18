using System;

namespace SweetSoft.QLDA.Core.EnumHelper
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ERenderAttribute : Attribute
    {
        public string DisplayName { get; }

        public ERenderAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EColorAttribute : Attribute
    {
        public string Color { get; }
        public EColorAttribute(string color)
        {
            Color = color;
        }
    }
}
