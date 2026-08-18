using SweetSoft.QLDA.Controls.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Controls
{
    public class EnumHelper
    {
        public static IEnumerable<T> EnumToList<T>() where T : struct
        {
            Type enumType = typeof(T);

            // Can't use generic type constraints on value types,
            // so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);
            List<T> enumValList = new List<T>();

            foreach (T val in enumValArray)
                enumValList.Add(val);

            return enumValList;
        }

        static readonly Random _Random = new Random();

        public static T RandomEnumValue<T>()
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .OrderBy(x => _Random.Next())
                .FirstOrDefault();
        }

        public static string ToRender(Enum enumerator)
        {
            Type type = enumerator.GetType();
            try
            {
                System.Reflection.FieldInfo fieldInfo = type.GetField(enumerator.ToString());
                RenderAttribute attribute = fieldInfo.GetCustomAttributes(typeof(RenderAttribute), false).FirstOrDefault() as RenderAttribute;
                if (attribute == null)
                    return enumerator.ToString();

                return attribute.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
