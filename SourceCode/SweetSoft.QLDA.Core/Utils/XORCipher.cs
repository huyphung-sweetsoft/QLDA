using Newtonsoft.Json;
using System;
using System.Text;

namespace SweetSoft.QLDA.Core.Utils
{
    public class XORCipher
    {
        private static readonly string KEY = "HgvyoA343sPHtrLA7c8syG2CaSyuhGko";

        public static string Encrypt(object text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text.ToString());
            byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
            byte[] encryptedBytes = new byte[textBytes.Length];

            for (int i = 0; i < textBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Convert.ToBase64String(encryptedBytes);
        }
        public static bool Decrypt<T>(string encryptedText, out T result)
        {
            result = default;

            if (string.IsNullOrEmpty(encryptedText))
                return false;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                if (encryptedBytes.Length == 0)
                    return false;

                byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
                byte[] decryptedBytes = new byte[encryptedBytes.Length];

                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
                }

                string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                if (typeof(T) == typeof(string))
                {
                    result = (T)(object)decryptedText;
                }
                else if (typeof(T) == typeof(Guid))
                {
                    if (Guid.TryParse(decryptedText, out Guid guid))
                    {
                        result = (T)(object)guid;
                    }
                    else
                        return false;
                }
                else if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal))
                {
                    result = (T)Convert.ChangeType(decryptedText, typeof(T));
                }
                else
                {
                    result = JsonConvert.DeserializeObject<T>(decryptedText);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
