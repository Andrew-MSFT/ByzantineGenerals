using ByzantineGenerals.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    public class MessageIn
    {
        public byte[] PreviousMessageHash { get; set; }
        public int PreviousMessageIdx { get; set; }
        public Decisions Decision { get; set; }
        public RSAParameters PublicKey { get; set; }
        public byte[] Signature { get; set; }
        
    }

    public class MessageOut
    {
        public Decisions Decision { get; set; }
        public byte[] RecipientKeyHash { get; set; }
    }

    public class Transaction
    {
        public List<MessageIn> Inputs { get; set; }
        public List<MessageOut> Outputs { get; set; }
    }

    public class Block
    {
        //Block Header
        public BigInteger Target { get; set; }
        public byte[] PreviousHash { get; set; }
        public List<Transaction> Messages { get; set; }
        public byte[] HashMessages { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Nonce { get; set; }


        private SHA256 _sHA256 = SHA256.Create();

        public Block(long index, List<Transaction> transactions, byte[] previousHash)
        {
            this.Target = HashUtilities.MaxTarget;
            this.Messages = transactions == null ? new List<Transaction>() : transactions;
            this.PreviousHash = previousHash;
            this.TimeStamp = DateTime.Now;
            //this.HashedTransactions = HashUtilities.ComputeSHA256(this.Transactions);

            CalculateNonce();
        }

        private void CalculateNonce()
        {
            bool nonceFound = false;
            for (int nonce = 0; nonce <= int.MaxValue; nonce++)
            {
                this.Nonce = nonce;

                if (ProofOfWorkIsValid(this))
                {
                    nonceFound = true;
                    break;
                }
            }

            if (!nonceFound)
            {
                throw new NotImplementedException("Implementation does not support no nonce for block structure");
            }
        }

        //public bool ContainsOutTransaction(byte[] transactionHash, out TransactionOut output)
        //{
        //    output = null;

        //    foreach (Transaction tx in this.Transactions)
        //    {
        //        foreach (TransactionOut txOut in tx.Outputs)
        //        {
        //            if (txOut.GetSHA256().SequenceEqual(transactionHash))
        //            {
        //                output = txOut;
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public byte[] GetSHA256()
        {
            byte[] powTarget = this.Target.ToByteArray();
            byte[] timeStamp = Block.ObjectToByteArray(this.TimeStamp);
            byte[] nonce = Block.ObjectToByteArray(this.Nonce);
            byte[] bytes = new byte[powTarget.Length + this.PreviousHash.Length + this.HashMessages.Length + timeStamp.Length + nonce.Length];

            int offSet = 0;
            Buffer.BlockCopy(powTarget, 0, bytes, offSet, powTarget.Length);
            offSet += powTarget.Length;
            Buffer.BlockCopy(this.PreviousHash, 0, bytes, offSet, this.PreviousHash.Length);
            offSet += this.PreviousHash.Length;
            Buffer.BlockCopy(this.HashMessages, 0, bytes, offSet, this.HashMessages.Length);
            offSet += this.HashMessages.Length;
            Buffer.BlockCopy(timeStamp, 0, bytes, offSet, timeStamp.Length);
            offSet += nonce.Length;
            Buffer.BlockCopy(nonce, 0, bytes, offSet, nonce.Length);

            byte[] hashBytes = _sHA256.ComputeHash(bytes);
            return hashBytes;
        }

        public bool ProofOfWorkIsValid(Block block)
        {
            byte[] hash = GetSHA256();
            BigInteger hashValue = new BigInteger(hash);
            BigInteger hashAbs = BigInteger.Abs(hashValue);
            return hashAbs < block.Target;
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
