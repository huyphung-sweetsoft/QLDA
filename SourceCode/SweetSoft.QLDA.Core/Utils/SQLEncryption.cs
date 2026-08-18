//-------------------------PROGRAMER LOGS------------------------
//**Change 01: Truong, 11 Nov 2024 - Update SQLEncryption
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SweetSoft.QLDA.Core.Utils
{
    public static class SQLEncryption
    {
        private static readonly string publickey = "yhgU4Lz8";
        private static readonly string secretkey = "wObN1zyc";
        public static string Encrypt(object textToEncrypt)
        {
            try
            {
                if (textToEncrypt == null)
                    return string.Empty;
                string ToReturn = "";
                byte[] secretkeyByte = { };
                secretkeyByte = Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = Encoding.UTF8.GetBytes(textToEncrypt.ToString());
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Decrypt(object textToDecrypt)
        {
            try
            {
                if (textToDecrypt == null)
                    return null;
                string ToReturn = "";
                byte[] privatekeyByte = { };
                privatekeyByte = Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.ToString().Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.ToString().Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static readonly int KeySize = 256; // AES-256
        private static readonly int BlockSize = 128; // Block size for AES
        private static readonly int Iterations = 11; // Number of iterations for PBKDF2
        private static readonly string password = "As921KX13dF94kgH";
        public static string EncryptAES256(object plainText)
        {
            if (plainText == null)
                throw new Exception("Input data is empty");
            // Generate a random IV
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.GenerateIV();
                aes.Key = GenerateKey(password, aes.IV);

                using (var ms = new MemoryStream())
                {
                    // Prepend the IV to the encrypted data
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        public static string DecryptAES256(object cipherText)
        {
            try
            {
                if (cipherText == null || string.IsNullOrEmpty(cipherText.ToString()))
                    throw new Exception("Encrypted value is null");
                byte[] fullCipher = Convert.FromBase64String(cipherText.ToString());

                // Get the IV from the start of the cipher text
                byte[] iv = new byte[BlockSize / 8];
                Array.Copy(fullCipher, iv, iv.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;
                    aes.Key = GenerateKey(password, iv);
                    aes.IV = iv;

                    using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                return cipherText.ToString();
            }
        }
        /// <summary>
        /// Change return value
        /// </summary>
        /// <param name="encryptedValue"></param>
        /// <returns></returns>
        public static decimal DecryptAndConvertToDecimal(object encryptedValue)
        {
            if (encryptedValue == null || string.IsNullOrEmpty(encryptedValue.ToString()))
                return 0;
            string decryptedString = DecryptAES256(encryptedValue.ToString());

            return decimal.TryParse(decryptedString, out var result) ? result : throw new Exception("Unable to parse value after decrypted");
        }
        public static int DecryptAndConvertToInteger(object encryptedValue)
        {
            if (encryptedValue == null || string.IsNullOrEmpty(encryptedValue.ToString()))
                throw new Exception("Encrypted value is null");
            string decryptedString = DecryptAES256(encryptedValue.ToString());

            return int.TryParse(decryptedString, out var result) ? result : throw new Exception("Unable to parse value after decrypted");
        }
        private static byte[] GenerateKey(string password, byte[] iv)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, iv, Iterations))
            {
                return pbkdf2.GetBytes(KeySize / 8);
            }
        }
    }

}
