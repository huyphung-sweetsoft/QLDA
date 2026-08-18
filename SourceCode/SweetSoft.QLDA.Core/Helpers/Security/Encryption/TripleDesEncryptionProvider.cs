using SweetSoft.QLDA.Core.Helpers.Interfaces;
using SweetSoft.QLDA.Core.Helpers.Security.Encryption;
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
    ///     Provides TripleDES based encryption and decryption services.
    /// </summary>
    public sealed class TripleDesEncryptionProvider : IEncryptionProvider
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly Encoding _encoding;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TripleDesEncryptionProvider"/> class.
        /// </summary>
        /// <param name="keyMaterial">The textual key material used to derive the symmetric key and initialization vector.</param>
        /// <param name="encoding">The character encoding applied when transforming text to bytes.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyMaterial"/> is null or shorter than sixteen characters.</exception>
        public TripleDesEncryptionProvider(string keyMaterial, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(keyMaterial) || keyMaterial.Length < 16)
            {
                throw new ArgumentException("TripleDES key material must contain at least sixteen characters.", nameof(keyMaterial));
            }

            _encoding = encoding ?? Encoding.Unicode;
            _key = Encoding.ASCII.GetBytes(keyMaterial.Substring(0, 16));
            _initializationVector = Encoding.ASCII.GetBytes(keyMaterial.Substring(8, 8));
        }

        /// <inheritdoc />
        public string AlgorithmName => EncryptionAlgorithmNames.TripleDes;

        /// <inheritdoc />
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText ?? string.Empty;
            }

            using (var provider = TripleDES.Create())
            {
                provider.Key = _key;
                provider.IV = _initializationVector;
                using (var encryptor = provider.CreateEncryptor())
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

            using (var provider = TripleDES.Create())
            {
                provider.Key = _key;
                provider.IV = _initializationVector;

                string sanitized = cipherText.Replace(" ", "+");
                byte[] buffer = Convert.FromBase64String(sanitized);

                using (var input = new MemoryStream(buffer))
                {
                    using (var decryptor = provider.CreateDecryptor())
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
