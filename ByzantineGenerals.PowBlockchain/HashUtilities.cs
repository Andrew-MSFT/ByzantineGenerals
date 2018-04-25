using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ByzantineGenerals.PowBlockchain
{
    public class HashUtilities
    {
        const int HashLength = 32;
        private static SHA256 _sHA256 = SHA256.Create();

        public static BigInteger MaxTarget
        {
            get
            {
                byte[] bytes = Enumerable.Repeat<byte>(255, HashLength).ToArray();
                bytes[HashLength - 1] = 127;
                return new BigInteger(bytes);
            }
        }
        public static BigInteger GetTargetHash(int hashDifficulty)
        {
            if (hashDifficulty == 0)
            {
                return MaxTarget;
            }

            byte[] bytes = Enumerable.Repeat<byte>(255, HashLength).ToArray();
            byte[] zeros = Enumerable.Repeat<byte>(0, hashDifficulty).ToArray();
            Buffer.BlockCopy(zeros, 0, bytes, HashLength - hashDifficulty, zeros.Length);
            return new BigInteger(bytes);
        }

        internal static byte[] ComputeSHA256(byte[] bytes)
        {
            var hashBytes = _sHA256.ComputeHash(bytes);
            return hashBytes;
        }

        internal static byte[] ComputeSHA256(object obj)
        {
            byte[] bytes = ObjectToByteArray(obj);
            byte[] hash = ComputeSHA256(bytes);
            return hash;
        }

        internal static byte[] ComputeSHA256(string serialized)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(serialized);
            byte[] hash = ComputeSHA256(bytes);
            return hash;
        }

        internal static byte[] ComputeSHA256(RSAParameters rsaKey)
        {
            string serializedKey = JsonConvert.SerializeObject(rsaKey);
            return ComputeSHA256(serializedKey);
        }

        // Convert an object to a byte array
        internal static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

    }
}
