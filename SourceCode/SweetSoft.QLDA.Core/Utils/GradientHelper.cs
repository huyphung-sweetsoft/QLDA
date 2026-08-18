using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Utils
{
    public class GradientHelper
    {
        private static readonly Random _rand = new Random();

        public static string GetRandomGradient()
        {
            string color1 = GetRandomColorHex();
            string color2 = GetRandomColorHex();
            int angle = _rand.Next(0, 361); // góc 0-360 độ

            return $"linear-gradient({angle}deg, {color1} 0%, {color2} 100%)";
        }

        private static string GetRandomColorHex()
        {
            return $"#{_rand.Next(0x1000000):X6}";
        }
    }
}
