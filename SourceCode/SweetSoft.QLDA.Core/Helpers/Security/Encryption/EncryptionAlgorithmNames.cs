using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Helpers.Security.Encryption
{
    /// <summary>
    ///     Provides the algorithm name constants used by the encryption infrastructure.
    /// </summary>
    public static class EncryptionAlgorithmNames
    {
        /// <summary>
        ///     Identifies the TripleDES encryption algorithm.
        /// </summary>
        public const string TripleDes = "TRIPLEDES";

        /// <summary>
        ///     Identifies the legacy DES encryption algorithm that is still used for backward compatible payloads.
        /// </summary>
        public const string LegacyDes = "DES";
    }
}
