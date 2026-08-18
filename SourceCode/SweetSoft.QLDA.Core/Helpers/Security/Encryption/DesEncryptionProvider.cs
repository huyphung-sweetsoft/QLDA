using SweetSoft.QLDA.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Security.Encryption
{
    /// <summary>
    ///     Provides DES based encryption and decryption services for backward compatible scenarios.
    /// </summary>
    public sealed class DesEncryptionProvider : IEncryptionProvider
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly Encoding _encoding;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DesEncryptionProvider"/> class.
        /// </summary>
        /// <param name="key">The key applied during encryption.</param>
        /// <param name="initializationVector">The initialization vector applied during encryption.</param>
        /// <param name="encoding">The character encoding applied when transforming text to bytes.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> or <paramref name="initializationVector"/> does not contain eight characters.</exception>
        public DesEncryptionProvider(string key, string initializationVector, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length != 8)
            {
                throw new ArgumentException("DES key material must contain exactly eight characters.", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(initializationVector) || initializationVector.Length != 8)
            {
                throw new ArgumentException("DES initialization vector must contain exactly eight characters.", nameof(initializationVector));
            }

            _encoding = encoding ?? Encoding.UTF8;
            _key = _encoding.GetBytes(key);
            _initializationVector = _encoding.GetBytes(initializationVector);
        }

        /// <inheritdoc />
        public string AlgorithmName => EncryptionAlgorithmNames.LegacyDes;

        /// <inheritdoc />
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText ?? string.Empty;
            }

            using (var provider = DES.Create())
            {
                using (var encryptor = provider.CreateEncryptor(_key, _initializationVector))
                {
                    using (var output = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] buffer = _encoding.GetBytes(plainText);
                            cryptoStream.Write(buffer, 0, buffer.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(output.ToArray());
                    }
                }
            }
        }

        /// <inheritdoc />
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return cipherText ?? string.Empty;
            }

            byte[] buffer = Convert.FromBase64String(cipherText.Replace(" ", "+"));

            using (var provider = DES.Create())
            {
                using (var decryptor = provider.CreateDecryptor(_key, _initializationVector))
                {
                    using (var input = new MemoryStream(buffer))
                    {
                        using (var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cryptoStream, _encoding))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
