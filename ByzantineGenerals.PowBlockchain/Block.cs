using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    public enum Decisions { NoneRecieved, Attack, Retreat }

    public struct MessageIn
    {
        public byte[] PreviousMessageHash { get; set; }
        public int PreviousMessageIdx { get; set; }
        public Decisions Decision { get; set; }
        public RSAParameters PublicKey { get; set; }
        public byte[] Signature { get; set; }

    }

    public struct MessageOut
    {
        public Decisions Decision { get; set; }
        public byte[] RecipientKeyHash { get; set; }

        public byte[] CalculateSHA256()
        {
            string serialized = JsonConvert.SerializeObject(this);
            byte[] thisHash = HashUtilities.ComputeSHA256(serialized);
            return thisHash;
        }
    }

    public struct Message
    {
        public List<MessageIn> Input { get; set; }
        public List<MessageOut> Outputs { get; set; }
    }

    public class Block
    {
        public static readonly byte[] DecisionInBaseHash = Enumerable.Repeat<byte>(0, 32).ToArray();
        public static readonly byte[] DecisionInSignature = Enumerable.Repeat<byte>(1, 32).ToArray();
        public const int DecisionInBaseIndex = -1;

        //Block Header
        public BigInteger Target { get; set; }
        public byte[] PreviousHash { get; set; }
        public List<Message> Messages { get; set; }
        public byte[] HashMessages { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Nonce { get; set; }


        private SHA256 _sHA256 = SHA256.Create();

        private Block(List<Message> transactions, byte[] previousHash)
        {
            this.Target = HashUtilities.MaxTarget;
            this.Messages = transactions;
            this.PreviousHash = previousHash;
            this.TimeStamp = DateTime.Now;
            this.HashMessages = ComputeMessagesSHA256();
        }

        private Block(Block block)
        {
            this.Target = block.Target;
            this.PreviousHash = (byte[])block.PreviousHash.Clone();
            this.TimeStamp = block.TimeStamp;
            this.HashMessages = (byte[])block.HashMessages.Clone();
            this.Messages = new List<Message>();

            foreach (var b in block.Messages)
            {
                Message message = b;
                this.Messages.Add(message);
            }
        }

        public static Block MineNewBlock(List<Message> transactions, byte[] previousHash)
        {
            Block block = new Block(transactions, previousHash);
            block.CalculateNonce();
            return block;
        }

        public static Block CopyBlock(Block block)
        {
            Block newBlock = new Block(block);
            return newBlock;
        }

        private byte[] ComputeMessagesSHA256()
        {
            string serialized = JsonConvert.SerializeObject(this.Messages);
            return HashUtilities.ComputeSHA256(serialized);
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

        public bool ContainsOutTransaction(byte[] transactionHash, out MessageOut output)
        {
            output = new MessageOut();

            foreach (Message tx in this.Messages)
            {
                foreach (MessageOut txOut in tx.Outputs)
                {
                    if (txOut.CalculateSHA256().SequenceEqual(transactionHash))
                    {
                        output = txOut;
                        return true;
                    }
                }
            }

            return false;
        }

        public byte[] ComputeSHA256()
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

        public bool Equals(Block block)
        {
            bool e = this.ComputeSHA256().SequenceEqual(block.ComputeSHA256());
            return e;
        }

        public bool ProofOfWorkIsValid(Block block)
        {
            byte[] hash = ComputeSHA256();
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
}
