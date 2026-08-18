using SweetSoft.QLDA.Core.Helpers.Interfaces;
using SweetSoft.QLDA.Core.Helpers.Security.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Security
{
    /// <summary>
    ///     Provides helper methods that centralize encryption, hashing, token generation and masking logic.
    /// </summary>
    public static class SecurityUtilities
    {
        /// <summary>
        ///     Identifies the application that leverages the utility helpers.
        /// </summary>
        public const string ApplicationName = "SweetSoft.QLDA.BackOffice";

        private const string DefaultTripleDesKey = "j+zqNUpaAm/Psqz0o77Gyg==";
        private const string LegacyDesKey = "EncryptK";
        private const string LegacyDesInitializationVector = "SITEMSCV";
        private const string NumericCharacters = "0123456789";
        private const string AlphaNumericCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly EncryptionService _encryptionService = CreateDefaultEncryptionService();

        /// <summary>
        ///     Gets the registered encryption algorithms supported by the utility.
        /// </summary>
        public static IReadOnlyCollection<string> RegisteredAlgorithms => _encryptionService.RegisteredAlgorithms;

        /// <summary>
        ///     Registers a new encryption provider, allowing consumers to extend the available algorithms.
        /// </summary>
        /// <param name="provider">The provider implementation to register.</param>
        /// <param name="setAsDefault">Whether the provider should become the new default algorithm.</param>
        public static void RegisterEncryptionProvider(IEncryptionProvider provider, bool setAsDefault = false)
        {
            _encryptionService.RegisterProvider(provider, setAsDefault);
        }

        /// <summary>
        ///     Defines the default encryption algorithm used by the encryption helpers when none is specified explicitly.
        /// </summary>
        /// <param name="algorithmName">The algorithm name to mark as default.</param>
        public static void SetDefaultEncryptionAlgorithm(string algorithmName)
        {
            _encryptionService.SetDefaultAlgorithm(algorithmName);
        }

        /// <summary>
        ///     Encrypts the provided text using either the default or a specifically requested algorithm.
        /// </summary>
        /// <param name="plainText">The input text that should be encrypted.</param>
        /// <param name="algorithmName">An optional algorithm name that overrides the default provider.</param>
        /// <returns>The encrypted representation of <paramref name="plainText"/>.</returns>
        public static string EncryptContent(string plainText, string algorithmName = null)
        {
            return ExecuteSafely(
                () => string.IsNullOrWhiteSpace(algorithmName)
                    ? _encryptionService.Encrypt(plainText)
                    : _encryptionService.Encrypt(plainText, algorithmName),
                plainText ?? string.Empty);
        }

        /// <summary>
        ///     Decrypts the provided text using either the default or a specifically requested algorithm.
        /// </summary>
        /// <param name="cipherText">The encrypted text that should be decrypted.</param>
        /// <param name="algorithmName">An optional algorithm name that overrides the default provider.</param>
        /// <returns>The decrypted representation of <paramref name="cipherText"/>.</returns>
        public static string DecryptContent(string cipherText, string algorithmName = null)
        {
            return ExecuteSafely(
                () => string.IsNullOrWhiteSpace(algorithmName)
                    ? _encryptionService.Decrypt(cipherText)
                    : _encryptionService.Decrypt(cipherText, algorithmName),
                cipherText ?? string.Empty);
        }

        /// <summary>
        ///     Creates a token value comprised of the current UTC timestamp and a GUID, encrypted with the legacy DES provider.
        /// </summary>
        /// <returns>The encrypted token that combines a timestamp and a unique identifier.</returns>
        public static string CreateEncryptedToken()
        {
            var payload = string.Format("{0}{1}", DateTime.UtcNow, Guid.NewGuid());
            return ExecuteSafely(
                () => _encryptionService.Encrypt(payload, EncryptionAlgorithmNames.LegacyDes),
                payload);
        }

        /// <summary>
        ///     Generates an expiring token that contains a timestamp and unique key bytes.
        /// </summary>
        /// <param name="minutes">The number of minutes the token should remain valid.</param>
        /// <returns>A byte array representing the encoded token, or <c>null</c> when the operation fails.</returns>
        public static byte[] CreateTimedToken(int minutes)
        {
            try
            {
                var effectiveMinutes = minutes <= 0 ? 1 : minutes;
                byte[] timeBytes = BitConverter.GetBytes(DateTime.UtcNow.AddMinutes(effectiveMinutes).ToBinary());
                byte[] keyBytes = Guid.NewGuid().ToByteArray();
                string token = Convert.ToBase64String(timeBytes.Concat(keyBytes).ToArray());
                return Convert.FromBase64String(token);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Creates an identifier composed of digits with an optional prefix.
        /// </summary>
        /// <param name="prefix">The textual prefix to prepend to the identifier. Defaults to <c>"A"</c>.</param>
        /// <param name="length">The number of numeric characters to generate.</param>
        /// <returns>A prefixed identifier that only contains digits.</returns>
        public static string CreateIdentifier(string prefix = "A", int length = 8)
        {
            string digits = CreateRandomCharacters(NumericCharacters, length);
            return string.IsNullOrWhiteSpace(prefix) ? digits : string.Concat(prefix, "-", digits);
        }

        /// <summary>
        ///     Creates a numeric code with the specified length.
        /// </summary>
        /// <param name="length">The number of digits included in the code.</param>
        /// <returns>A string that only consists of numeric characters.</returns>
        public static string CreateNumericCode(int length)
        {
            return CreateRandomCharacters(NumericCharacters, length);
        }

        /// <summary>
        ///     Generates a random alphanumeric string and optionally hashes the result using MD5.
        /// </summary>
        /// <param name="length">The number of characters to generate.</param>
        /// <param name="hashAsMd5">Indicates whether the generated string should be hashed using MD5.</param>
        /// <returns>Either the generated alphanumeric string or its MD5 hash.</returns>
        public static string CreateAlphaNumericString(int length, bool hashAsMd5 = false)
        {
            string value = CreateRandomCharacters(AlphaNumericCharacters, length);
            return hashAsMd5 ? ComputeMd5Hash(value) : value;
        }

        /// <summary>
        ///     Computes the MD5 hash for the provided input concatenated with the encryption key.
        /// </summary>
        /// <param name="input">The text for which the hash should be computed.</param>
        /// <returns>The hexadecimal MD5 hash value.</returns>
        public static string ComputeMd5Hash(string input)
        {
            string valueToHash = (input ?? string.Empty) + DefaultTripleDesKey;

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));

                var builder = new StringBuilder(data.Length * 2);
                foreach (byte b in data)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        ///     Validates whether the provided hash is a match for the supplied input.
        /// </summary>
        /// <param name="input">The text for which the hash was created.</param>
        /// <param name="hash">The hash value that should be compared.</param>
        /// <returns><c>true</c> when the hash matches the input; otherwise, <c>false</c>.</returns>
        public static bool IsMd5HashValid(string input, string hash)
        {
            try
            {
                string computedHash = ComputeMd5Hash(input);
                return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Encrypts text and returns a URL safe representation of the cipher text.
        /// </summary>
        /// <param name="plainText">The text that should be encrypted.</param>
        /// <returns>A URL safe encrypted string.</returns>
        public static string ProtectUrlParameter(string plainText)
        {
            if (plainText is null)
            {
                return string.Empty;
            }

            string cipher = EncryptContent(plainText, EncryptionAlgorithmNames.LegacyDes);
            return cipher.Replace("/", "_").Replace("=", string.Empty);
        }

        /// <summary>
        ///     Decrypts a URL safe encrypted string back to its original value.
        /// </summary>
        /// <param name="cipherText">The encrypted value produced by <see cref="ProtectUrlParameter"/>.</param>
        /// <returns>The original plain text value.</returns>
        public static string UnprotectUrlParameter(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                {
                    return string.Empty;
                }

                string base64 = cipherText.Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2:
                        base64 += "==";
                        break;
                    case 3:
                        base64 += "=";
                        break;
                }

                return DecryptContent(base64, EncryptionAlgorithmNames.LegacyDes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     Masks a string by replacing all but the last characters with asterisks.
        /// </summary>
        /// <param name="value">The value that should be partially obscured.</param>
        /// <param name="visibleLength">The number of characters to keep visible at the end of the string.</param>
        /// <returns>The masked value.</returns>
        public static string MaskValue(string value, int visibleLength = 4)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                if (visibleLength <= 0 || visibleLength >= value.Length)
                {
                    return value;
                }

                string masked = new string('*', value.Length - visibleLength);
                return masked + value.Substring(value.Length - visibleLength);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        ///     Generates a random string based on the supplied allowed characters.
        /// </summary>
        /// <param name="allowedCharacters">The characters that can appear in the generated value.</param>
        /// <param name="length">The number of characters to generate.</param>
        /// <returns>A random string composed from <paramref name="allowedCharacters"/>.</returns>
        private static string CreateRandomCharacters(string allowedCharacters, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
            }

            var result = new char[length];
            var randomBytes = new byte[length];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    result[i] = allowedCharacters[randomBytes[i] % allowedCharacters.Length];
                }

                return new string(result);
            }
        }

        /// <summary>
        ///     Executes the provided action while ensuring that failures fall back to a default value.
        /// </summary>
        /// <param name="action">The operation that should be executed.</param>
        /// <param name="fallback">The fallback value returned when the action throws an exception.</param>
        /// <returns>The result of <paramref name="action"/> or the fallback value when an error occurs.</returns>
        private static string ExecuteSafely(Func<string> action, string fallback)
        {
            try
            {
                return action();
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        ///     Creates the encryption service pre-configured with the standard providers used by the application.
        /// </summary>
        /// <returns>An initialized <see cref="EncryptionService"/> instance.</returns>
        private static EncryptionService CreateDefaultEncryptionService()
        {
            var providers = new List<IEncryptionProvider>
            {
                new TripleDesEncryptionProvider(DefaultTripleDesKey),
                new DesEncryptionProvider(LegacyDesKey, LegacyDesInitializationVector)
            };

            return new EncryptionService(EncryptionAlgorithmNames.TripleDes, providers);
        }
    }
}
