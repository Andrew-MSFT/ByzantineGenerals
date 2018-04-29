using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("ByzantineGenerals.Pow.Tests")]


namespace ByzantineGenerals.PowBlockchain
{
    public class General
    {
        public Decisions Decision { get; private set; }
        public RSAParameters PublicKey { get; private set; }
        public Blockchain MessageChain { get; private set; }
        internal List<Message> RecievedMessagePool { get; private set; } = new List<Message>();
        internal List<Block> RecievedBlockPool { get; private set; } = new List<Block>();

        private Dictionary<RSAParameters, List<Message>> _inputQueue = new Dictionary<RSAParameters, List<Message>>();
        private Block _myBlock;
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();

        internal General(Decisions decision, Blockchain currentChain)
        {
            this.Decision = decision;
            this.PublicKey = _rSA.ExportParameters(false);
            this.MessageChain = new Blockchain(currentChain);
        }

        public void DeclareIninitialPreference()
        {
            Message initialDecision = Message.CreateBaseDecision(this.Decision, this.PublicKey);
            List<Message> transactions = new List<Message> { initialDecision };
            byte[] previousHash = MessageChain.LastBlock.ComputeSHA256();
            _myBlock = Block.MineNewBlock(transactions, previousHash);

            FinishedMiningBlock(_myBlock);
        }

        public void Coordinate()
        {
            List<MessageOut> publicDecisions = new List<MessageOut>();
            List<MessageOut> inputs = new List<MessageOut> { _myBlock.Messages[0].Outputs[0] };

            foreach (var general in CommandService.GetOtherGenerals(this.PublicKey))
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {

                    MessageOut message = new MessageOut(this.Decision,general.PublicKey);
                    publicDecisions.Add(message);
                }
            }

            Message broadCastMessage = Message.CreateNewMessage(inputs, publicDecisions, this);

            CommandService.BroadCastDecision(broadCastMessage, this.PublicKey);
        }


        public void NotifyBlockMined(Messenger messenger)
        {
            Block block = messenger.MinedBlock;
            this.RecievedBlockPool.Add(block);
            this.MessageChain?.Add(block);
        }

        public void RecieveMessage(Messenger messenger)
        {
            Message message = messenger.Message;
            this.RecievedMessagePool.Add(message);
            //_inputQueue.Add(message.Input.PublicKey, message);
            //if (_inputQueue.Count == _messageService.GetOtherGenerals(this.PublicKey).Count)
            //{
            //    //MineBlock();
            //}
        }

        private void MineBlock(List<Message> transactions)
        {
            byte[] previousHash = MessageChain.LastBlock.ComputeSHA256();
            Block block = Block.MineNewBlock(transactions, previousHash);

            FinishedMiningBlock(block);
        }

        private void FinishedMiningBlock(Block block)
        {
            this.MessageChain.Add(block);
            CommandService.NotifyNewBlockMined(block, this.PublicKey);
        }



        public byte[] SignMessage(MessageOut message)
        {
            string serialized = JsonConvert.SerializeObject(message);
            byte[] hashedValue = HashUtilities.ComputeSHA256(serialized);
            //Create an RSAPKCS1SignatureFormatter object and pass it the   
            //RSACryptoServiceProvider to transfer the private key.  
            RSAPKCS1SignatureFormatter rSAFormatter = new RSAPKCS1SignatureFormatter(_rSA);

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
