using SweetSoft.QLDA.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Security.Encryption
{
    /// <summary>
    ///     Coordinates encryption providers and exposes a simple API to encrypt or decrypt text using registered algorithms.
    /// </summary>
    public sealed class EncryptionService
    {
        private readonly Dictionary<string, IEncryptionProvider> _providers;
        private readonly object _syncRoot = new object();
        private string _defaultAlgorithmName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptionService"/> class with the provided default algorithm.
        /// </summary>
        /// <param name="defaultAlgorithmName">The algorithm name used when a consumer does not specify one explicitly.</param>
        /// <param name="providers">The providers registered during construction.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="providers"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="defaultAlgorithmName"/> is not registered.</exception>
        public EncryptionService(string defaultAlgorithmName, IEnumerable<IEncryptionProvider> providers)
        {
            if (providers is null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _providers = new Dictionary<string, IEncryptionProvider>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                RegisterProviderInternal(provider, setAsDefault: false);
            }

            if (_providers.Count == 0)
            {
                throw new ArgumentException("At least one encryption provider must be registered.", nameof(providers));
            }

            SetDefaultAlgorithm(defaultAlgorithmName);
        }

        /// <summary>
        ///     Gets the name of the algorithm applied when no explicit name is provided.
        /// </summary>
        public string DefaultAlgorithmName
        {
            get
            {
                lock (_syncRoot)
                {
                    return _defaultAlgorithmName;
                }
            }
        }

        /// <summary>
        ///     Gets the registered algorithm names.
        /// </summary>
        public IReadOnlyCollection<string> RegisteredAlgorithms
        {
            get
            {
                lock (_syncRoot)
                {
                    return _providers.Keys.ToArray();
                }
            }
        }

        /// <summary>
        ///     Registers a new encryption provider that can later be used by the service.
        /// </summary>
        /// <param name="provider">The provider to register.</param>
        /// <param name="setAsDefault">Whether the provider should become the new default algorithm.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is null.</exception>
        public void RegisterProvider(IEncryptionProvider provider, bool setAsDefault = false)
        {
            RegisterProviderInternal(provider, setAsDefault);
        }

        /// <summary>
        ///     Sets the algorithm name that should be used as default when consumers do not specify one.
        /// </summary>
        /// <param name="algorithmName">The name of the algorithm to use as default.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="algorithmName"/> is null or not registered.</exception>
        public void SetDefaultAlgorithm(string algorithmName)
        {
            if (string.IsNullOrWhiteSpace(algorithmName))
            {
                throw new ArgumentException("Default algorithm name must be provided.", nameof(algorithmName));
            }

            lock (_syncRoot)
            {
                if (!_providers.ContainsKey(algorithmName))
                {
                    throw new KeyNotFoundException($"Algorithm '{algorithmName}' has not been registered.");
                }

                _defaultAlgorithmName = algorithmName;
            }
        }

        /// <summary>
        ///     Encrypts the provided text using the default algorithm.
        /// </summary>
        /// <param name="plainText">The input text that should be protected.</param>
        /// <returns>The encrypted representation of <paramref name="plainText"/>.</returns>
        public string Encrypt(string plainText)
        {
            return Encrypt(plainText, DefaultAlgorithmName);
        }

        /// <summary>
        ///     Encrypts the provided text using the specified algorithm.
        /// </summary>
        /// <param name="plainText">The input text that should be protected.</param>
        /// <param name="algorithmName">The algorithm name to apply.</param>
        /// <returns>The encrypted representation of <paramref name="plainText"/>.</returns>
        public string Encrypt(string plainText, string algorithmName)
        {
            return ResolveProvider(algorithmName).Encrypt(plainText);
        }

        /// <summary>
        ///     Decrypts the provided text using the default algorithm.
        /// </summary>
        /// <param name="cipherText">The encrypted text that should be decoded.</param>
        /// <returns>The decrypted value of <paramref name="cipherText"/>.</returns>
        public string Decrypt(string cipherText)
        {
            return Decrypt(cipherText, DefaultAlgorithmName);
        }

        /// <summary>
        ///     Decrypts the provided text using the specified algorithm.
        /// </summary>
        /// <param name="cipherText">The encrypted text that should be decoded.</param>
        /// <param name="algorithmName">The algorithm name to apply.</param>
        /// <returns>The decrypted value of <paramref name="cipherText"/>.</returns>
        public string Decrypt(string cipherText, string algorithmName)
        {
            return ResolveProvider(algorithmName).Decrypt(cipherText);
        }

        /// <summary>
        ///     Resolves the provider associated with the supplied algorithm name.
        /// </summary>
        /// <param name="algorithmName">The algorithm name whose provider should be returned.</param>
        /// <returns>The provider registered for <paramref name="algorithmName"/> or the default provider when no name is supplied.</returns>
        private IEncryptionProvider ResolveProvider(string algorithmName)
        {
            var resolvedAlgorithmName = string.IsNullOrWhiteSpace(algorithmName) ? DefaultAlgorithmName : algorithmName;

            lock (_syncRoot)
            {
                if (!_providers.TryGetValue(resolvedAlgorithmName, out var provider))
                {
                    throw new KeyNotFoundException($"Algorithm '{resolvedAlgorithmName}' has not been registered.");
                }

                return provider;
            }
        }

        /// <summary>
        ///     Adds or replaces a provider inside the internal collection.
        /// </summary>
        /// <param name="provider">The provider to register.</param>
        /// <param name="setAsDefault">Determines whether the provider becomes the default algorithm.</param>
        private void RegisterProviderInternal(IEncryptionProvider provider, bool setAsDefault)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            lock (_syncRoot)
            {
                _providers[provider.AlgorithmName] = provider;

                if (setAsDefault)
                {
                    _defaultAlgorithmName = provider.AlgorithmName;
                }
            }
        }
    }
}
