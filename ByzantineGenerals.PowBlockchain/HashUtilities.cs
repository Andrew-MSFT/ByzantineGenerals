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

        public static byte[] SignMessage(MessageOut message, RSACryptoServiceProvider rsaProvider)
        {
            string serialized = JsonConvert.SerializeObject(message);
            byte[] hashedValue = HashUtilities.ComputeSHA256(serialized);
            //Create an RSAPKCS1SignatureFormatter object and pass it the   
            //RSACryptoServiceProvider to transfer the private key.  
            RSAPKCS1SignatureFormatter rSAFormatter = new RSAPKCS1SignatureFormatter(rsaProvider);

            //Set the hash algorithm  
            rSAFormatter.SetHashAlgorithm("SHA256");

            //Create a signature for hashValue
            byte[] signedHashValue = rSAFormatter.CreateSignature(hashedValue);
            return signedHashValue;
        }

        public static bool VerifySignature(RSAParameters publicKey, byte[] originalData, byte[] signedHash)
        {
            RSACryptoServiceProvider rSA = new RSACryptoServiceProvider();
            rSA.ImportParameters(publicKey);
            RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(rSA);
            RSADeformatter.SetHashAlgorithm("SHA256");
            bool signatureIsValid = RSADeformatter.VerifySignature(originalData, signedHash);
            return signatureIsValid;
        }

    }
}
