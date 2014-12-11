using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace SharpTools.Configuration.Internal
{
    using SharpTools.Configuration.Crypto;

    /// <summary>
    /// This class provides a simplified API for symmetric encryption/decryption of text.
    ///
    /// By default, it will use Salsa20 for the symmetric encryption algorithm, and SHA512
    /// for hashing, but this can be configured by providing a CryptoAlgorithm instance to
    /// the appropriate constructor.
    ///
    /// The key for the algorithm will be derived from the password provided + a salt, which
    /// is derived by hashing the password once, then rehashing the hash iteratively 6 more
    /// times. This is not perfect, but will provide an extra layer of security over just the
    /// unsalted password by ensuring there is at least 128 bits available for deriving the key,
    /// and 512 if you use the defaults.
    ///
    /// Decrypting and encrypting expects and returns valid unicode-encoded strings, if you are
    /// working with a different encoding, then make sure you set the Encoding property to that
    /// encoding - otherwise you may get strange results/corruption of data.
    /// </summary>
    public class CryptoProvider
    {
        private byte[] _key;
        private SymmetricAlgorithm _algorithm;

        /// <summary>
        /// The encoding to use when encrypting/decrypting text.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Creates a new CryptoProvider instance using Salsa20/SHA512.
        /// </summary>
        /// <param name="password">The password to use when deriving the encryption key.</param>
        public CryptoProvider(string password) : this(password, CryptoAlgorithm.Salsa20) { }

        /// <summary>
        /// Creates a new CryptoProvider instance, using the provided CryptoAlgorithm.
        /// </summary>
        /// <param name="password">The password to use when deriving the encryption key.</param>
        /// <param name="algorithm">The CryptoAlgorithm instance which defines what algorithms to use when encrypting/hashing.</param>
        public CryptoProvider(string password, CryptoAlgorithm algorithm)
        {
            Encoding = Encoding.Unicode;

            // Create algorithm instance
            _algorithm = algorithm.CreateSymmetric();

            // Derive the key for this algorithm
            _key = DeriveKey(
                password,
                algorithm.SymmetricAlgorithm,
                algorithm.HashingAlgorithm,
                _algorithm.KeySize,
                _algorithm.IV
            );

            // Prepare the algorithm with the derived key
            _algorithm.Key = _key;
        }

        /// <summary>
        /// Encrypts the provided value, and returns the ciphertext as a string.
        /// </summary>
        /// <param name="value">The string to encrypt</param>
        /// <returns>An encrypted string</returns>
        public string Encrypt(string value)
        {
            using (var encryptor    = _algorithm.CreateEncryptor())
            using (var result       = new MemoryStream())
            using (var cryptoStream = new CryptoStream(result, encryptor, CryptoStreamMode.Write))
            {
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(value);
                }

                var bytes = result.ToArray();
                return Encoding.GetString(bytes);
            }
        }

        /// <summary>
        /// Decrypts the provided ciphertext, and returns the decrypted value as a string.
        /// </summary>
        /// <param name="ciphertext">The encrypted string to decrypt.</param>
        /// <returns>A decrypted string</returns>
        public string Decrypt(string ciphertext)
        {
            var bytes = Encoding.GetBytes(ciphertext);

            string decrypted;
            using (var decryptor    = _algorithm.CreateDecryptor())
            using (var result       = new MemoryStream(bytes))
            using (var cryptoStream = new CryptoStream(result, decryptor, CryptoStreamMode.Read))
            {
                using (var reader = new StreamReader(cryptoStream))
                {
                    decrypted = reader.ReadToEnd();
                }
            }

            return decrypted;
        }

        /// <summary>
        /// Derive key material from a provided password and crypto service provider.
        /// </summary>
        /// <param name="password">The password from which the key material will be derived</param>
        /// <param name="algorithm">The name of the symmetric algorithm to use.</param>
        /// <param name="hashAlgorithm">The name of the hashing algorithm to use.</param>
        /// <param name="keySize">The key size to use.</param>
        /// <param name="iv">The initialization vector to use.</param>
        /// <returns>The derived key as an array of bytes.</returns>
        private byte[] DeriveKey(string password, string algorithm, string hashAlgorithm, int keySize, byte[] iv)
        {
            var passwordBytes = Encoding.GetBytes(password);
            var saltBytes     = DeriveSalt(passwordBytes);

            var deriveBytes = new PasswordDeriveBytes(passwordBytes, saltBytes);
            var key = deriveBytes.CryptDeriveKey(algorithm, hashAlgorithm, keySize, iv);

            ClearKeyMaterial(passwordBytes);
            ClearKeyMaterial(saltBytes);

            return key;
        }

        /// <summary>
        /// Derives a salt from some input.
        /// </summary>
        /// <param name="input">The input bytes to derive the salt from</param>
        private static byte[] DeriveSalt(byte[] input)
        {
            // The number of re-hashes to perform
            var nRehashes = 6;
            // Hash with SHA-512
            using (var hasher = new SHA512Cng())
            {
                hasher.Initialize();

                // Hash the input, then rehash `n` times.
                var bytes = hasher.ComputeHash(input);
                for (var i = 0; i < nRehashes; i++)
                {
                    bytes = hasher.ComputeHash(bytes);
                }

                return bytes;
            }
        }

        /// <summary>
        /// Zeroes out a byte[] buffer's contents to remove key derivation material from memory
        /// </summary>
        /// <param name="buffer">The buffer to clear</param>
        private static void ClearKeyMaterial(byte[] buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
        }
    }
}
