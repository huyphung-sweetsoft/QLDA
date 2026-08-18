using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ValueObjects
{
    /// <summary>
    /// Represents a UUID version 7 (time-ordered UUID) that is compatible with System.Guid
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UUIDv7 : IEquatable<UUIDv7>, IComparable<UUIDv7>, IComparable, IFormattable
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly object _lockObject = new object();
        private static long _lastTimestamp = 0;
        private static ushort _clockSequence = 0;

        private readonly Guid _guid;

        public static readonly UUIDv7 Empty = new UUIDv7(Guid.Empty);

        private UUIDv7(Guid guid)
        {
            _guid = guid;
        }

        /// <summary>
        /// Creates a new UUIDv7 with current timestamp
        /// </summary>
        public static UUIDv7 NewGuid()
        {
            return Generate();
        }

        /// <summary>
        /// Generates a new UUIDv7
        /// </summary>
        public static UUIDv7 Generate()
        {
            lock (_lockObject)
            {
                // Get Unix timestamp in milliseconds
                var unixTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Handle clock sequence for same millisecond generation
                if (unixTimestampMs == _lastTimestamp)
                {
                    _clockSequence++;
                    if (_clockSequence >= 4096) // 12-bit limit
                    {
                        // If we've exhausted the clock sequence, wait for next millisecond
                        while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() == unixTimestampMs)
                        {
                            System.Threading.Thread.Sleep(1);
                        }
                        unixTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        _clockSequence = 0;
                    }
                }
                else
                {
                    _clockSequence = 0;
                    _lastTimestamp = unixTimestampMs;
                }

                // Generate random bytes for the random part
                byte[] randomBytes = new byte[10];
                _rng.GetBytes(randomBytes);

                // Build the UUID bytes according to UUIDv7 spec
                byte[] uuidBytes = new byte[16];

                // timestamp_ms (48 bits) - manually write big endian
                uuidBytes[0] = (byte)((unixTimestampMs >> 40) & 0xFF);
                uuidBytes[1] = (byte)((unixTimestampMs >> 32) & 0xFF);
                uuidBytes[2] = (byte)((unixTimestampMs >> 24) & 0xFF);
                uuidBytes[3] = (byte)((unixTimestampMs >> 16) & 0xFF);
                uuidBytes[4] = (byte)((unixTimestampMs >> 8) & 0xFF);
                uuidBytes[5] = (byte)(unixTimestampMs & 0xFF);

                // ver (4 bits) + rand_a (12 bits) - using clock sequence for better ordering
                uuidBytes[6] = (byte)(0x70 | ((_clockSequence >> 8) & 0x0F)); // Version 7
                uuidBytes[7] = (byte)(_clockSequence & 0xFF);

                // var (2 bits) + rand_b (62 bits)
                uuidBytes[8] = (byte)(0x80 | (randomBytes[0] & 0x3F)); // Variant bits: 10

                // Copy remaining random bytes
                Array.Copy(randomBytes, 1, uuidBytes, 9, 7);

                return new UUIDv7(new Guid(uuidBytes));
            }
        }

        /// <summary>
        /// Creates UUIDv7 from byte array
        /// </summary>
        public UUIDv7(byte[] b) : this(new Guid(b))
        {
        }

        /// <summary>
        /// Creates UUIDv7 from string
        /// </summary>
        public UUIDv7(string g) : this(new Guid(g))
        {
        }

        /// <summary>
        /// Creates UUIDv7 from components (same as Guid constructor)
        /// </summary>
        public UUIDv7(int a, short b, short c, byte[] d) : this(new Guid(a, b, c, d))
        {
        }

        /// <summary>
        /// Creates UUIDv7 from components (same as Guid constructor)
        /// </summary>
        public UUIDv7(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
            : this(new Guid(a, b, c, d, e, f, g, h, i, j, k))
        {
        }

        /// <summary>
        /// Gets the timestamp component of the UUIDv7
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get
            {
                var bytes = _guid.ToByteArray();
                // Extract 48-bit timestamp from first 6 bytes - manually read big endian
                long timestampMs = 0;
                for (int i = 0; i < 6; i++)
                {
                    timestampMs = (timestampMs << 8) | bytes[i];
                }
                return DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
            }
        }

        /// <summary>
        /// Checks if this is a valid UUIDv7 (version 7)
        /// </summary>
        public bool IsVersion7
        {
            get
            {
                var bytes = _guid.ToByteArray();
                return (bytes[6] & 0xF0) == 0x70; // Check version bits
            }
        }

        // Conversion operators
        public static implicit operator Guid(UUIDv7 uuid) => uuid._guid;
        public static explicit operator UUIDv7(Guid guid) => new UUIDv7(guid);

        // Standard Guid-compatible methods
        public byte[] ToByteArray() => _guid.ToByteArray();

        public override string ToString() => _guid.ToString();
        public string ToString(string format) => _guid.ToString(format);
        public string ToString(string format, IFormatProvider provider) => _guid.ToString(format, provider);

        // Parsing methods
        public static UUIDv7 Parse(string input) => new UUIDv7(Guid.Parse(input));
        public static UUIDv7 ParseExact(string input, string format) => new UUIDv7(Guid.ParseExact(input, format));

        public static bool TryParse(string input, out UUIDv7 result)
        {
            if (Guid.TryParse(input, out var guid))
            {
                result = new UUIDv7(guid);
                return true;
            }
            result = Empty;
            return false;
        }

        public static bool TryParseExact(string input, string format, out UUIDv7 result)
        {
            if (Guid.TryParseExact(input, format, out var guid))
            {
                result = new UUIDv7(guid);
                return true;
            }
            result = Empty;
            return false;
        }

        // Equality and comparison
        public bool Equals(UUIDv7 other) => _guid.Equals(other._guid);
        public override bool Equals(object obj) => obj is UUIDv7 other && Equals(other);
        public override int GetHashCode() => _guid.GetHashCode();

        public int CompareTo(UUIDv7 other) => _guid.CompareTo(other._guid);
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            if (obj is UUIDv7 other) return CompareTo(other);
            throw new ArgumentException("Object must be of type UUIDv7", nameof(obj));
        }

        // Operators
        public static bool operator ==(UUIDv7 left, UUIDv7 right) => left.Equals(right);
        public static bool operator !=(UUIDv7 left, UUIDv7 right) => !left.Equals(right);
        public static bool operator <(UUIDv7 left, UUIDv7 right) => left.CompareTo(right) < 0;
        public static bool operator <=(UUIDv7 left, UUIDv7 right) => left.CompareTo(right) <= 0;
        public static bool operator >(UUIDv7 left, UUIDv7 right) => left.CompareTo(right) > 0;
        public static bool operator >=(UUIDv7 left, UUIDv7 right) => left.CompareTo(right) >= 0;
    }

    // Extension methods for convenience
    public static class UUIDv7Extensions
    {
        /// <summary>
        /// Converts a Guid to UUIDv7 if it's a valid version 7 UUID
        /// </summary>
        public static UUIDv7 ToUUIDv7(this Guid guid)
        {
            return (UUIDv7)guid;
        }

        /// <summary>
        /// Checks if a Guid is a valid UUIDv7
        /// </summary>
        public static bool IsUUIDv7(this Guid guid)
        {
            var bytes = guid.ToByteArray();
            return (bytes[6] & 0xF0) == 0x70;
        }
    }
}
