using ByzantineGenerals.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ByzantineGenerals.PowBlockchain
{
    class General
    {
        public List<Block> Blockchain { get; private set; } = new List<Block>();
        public Decisions Decision { get; private set; }
        public RSAParameters PublicKey { get; private set; }

        private List<General> _generals;
        private List<Transaction> _inputQueue = new List<Transaction>();
        private Block _myBlock;

        public General(List<General> generals)
        {
            _generals = generals;

        }

        private void DeclareBase()
        {
            byte[] decisionInBaseHash = Enumerable.Repeat<byte>(0, 32).ToArray();
            MessageIn baseMessageIn = new MessageIn
            {
                Decision = this.Decision,
                PreviousMessageHash = decisionInBaseHash,
                PreviousMessageIdx = -1,
                PublicKey = this.PublicKey,
                Signature = decisionInBaseHash
            };
            MessageOut messageOut = new MessageOut
            {
                Decision = this.Decision,
                RecipientKeyHash = HashUtilities.ComputeSHA256(this.PublicKey)
            };
            List<MessageIn> messageIns = new List<MessageIn> { baseMessageIn };
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            List<Transaction> transactions = new List<Transaction> { new Transaction { Inputs = messageIns, Outputs = messageOuts } };
            Block block = new Block(this.Blockchain.Count, transactions, GetPreviousHash());
            _myBlock = block;
            FinishedMiningBlock(block);
        }

        public void Coordinate()
        {
            DeclareBase();

            MessageOut inputMessage = _myBlock.Messages[0].Outputs[0];
            byte[] sig = Sign(HashUtilities.ObjectToByteArray(inputMessage));
            MessageIn messageIn = new MessageIn
            {
                Decision = this.Decision,
                PreviousMessageIdx = 0,
                PreviousMessageHash = HashUtilities.ComputeSHA256(inputMessage),
                PublicKey = this.PublicKey,
                Signature = sig
            };
            List<MessageIn> inputs = new List<MessageIn>();
            List<MessageOut> decisions = new List<MessageOut>();

            foreach (var general in _generals)
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {
                   
                    MessageOut message = new MessageOut
                    {
                        Decision = this.Decision,
                        RecipientKeyHash = HashUtilities.ComputeSHA256(general.PublicKey)
                    };
                    decisions.Add(message);
                }
            }
            Transaction tx = new Transaction { Inputs = inputs, Outputs = decisions };
            BroadCastDecision(tx);
        }

        public byte[] Sign(byte[] valueToSign)
        {
            var rSA = new RSACryptoServiceProvider();
            byte[] hashedValue = HashUtilities.ComputeSHA256(valueToSign);
            //Create an RSAPKCS1SignatureFormatter object and pass it the   
            //RSACryptoServiceProvider to transfer the private key.  
            RSAPKCS1SignatureFormatter rSAFormatter = new RSAPKCS1SignatureFormatter(rSA);

            //Set the hash algorithm  
            rSAFormatter.SetHashAlgorithm("SHA256");

            //Create a signature for hashValue
            byte[] signedHashValue = rSAFormatter.CreateSignature(hashedValue);
            return signedHashValue;
        }

        private void BroadCastDecision(Transaction message)
        {
            foreach (var general in _generals)
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {
                    general.RecieveMessage(message);
                }
            }
        }

        private void NotifyBlockMined(Block block)
        {
            bool messageFound = true;
            foreach (var message in block.Messages)
            {
                //var localMessage = _inputQueue.Where(m => m.RecipientPublicKey message.RecipientPublicKey).First();
            }
        }

        public void RecieveMessage(Transaction message)
        {
            _inputQueue.Add(message);
            if (_inputQueue.Count == _generals.Count)
            {
                MineBlock();
            }
        }

        private void MineBlock()
        {
            //var transaction = new Transaction { Inputs = null, Outputs = _inputQueue };
            //byte[] previousHash;
            //previousHash = GetPreviousHash();

            //List<Transaction> transactions = new List<Transaction>();
            //var block = new Block(this.Blockchain.Count, transactions, previousHash);

            //FinishedMiningBlock(block);
        }

        private void FinishedMiningBlock(Block block)
        {
            this.Blockchain.Add(block);

            foreach (var general in _generals)
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {
                    general.NotifyBlockMined(block);
                }
            }
        }

        private byte[] GetPreviousHash()
        {
            byte[] previousHash;
            if (this.Blockchain.Count > 0)
            {
                previousHash = this.Blockchain[this.Blockchain.Count - 1].GetSHA256();
            }
            else
            {
                previousHash = Enumerable.Repeat<byte>(0, 32).ToArray();
            }

            return previousHash;
        }
    }
}
