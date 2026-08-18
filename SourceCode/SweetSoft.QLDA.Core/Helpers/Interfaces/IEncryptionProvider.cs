using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Interfaces
{
    /// <summary>
    ///     Defines the behavior required for an encryption provider implementation.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        ///     Gets the unique algorithm name handled by the provider. The value is case insensitive when registered.
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        ///     Encrypts the provided text using the configured algorithm options.
        /// </summary>
        /// <param name="plainText">The input text that should be protected.</param>
        /// <returns>The encrypted representation of <paramref name="plainText"/>.</returns>
        string Encrypt(string plainText);

        /// <summary>
        ///     Decrypts the provided text using the configured algorithm options.
        /// </summary>
        /// <param name="cipherText">The encrypted text that should be decoded.</param>
        /// <returns>The decrypted value of <paramref name="cipherText"/>.</returns>
        string Decrypt(string cipherText);
    }
}
