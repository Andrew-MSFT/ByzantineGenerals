using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ByzantineGenerals.Pow.Tests")]


namespace ByzantineGenerals.PowBlockchain
{
    public class General
    {
        public Decisions Decision { get; private set; }
        public RSAParameters PublicKey { get; private set; }
        public Blockchain MessageChain { get; private set; }
        internal List<Message> RecievedMessagePool { get; private set; } = new List<Message>();
        internal List<Message> OrphanedMessagePool { get; private set; } = new List<Message>();
        internal List<Block> RecievedBlockPool { get; private set; } = new List<Block>();
        internal List<Block> OrphanedBlockPool { get; private set; } = new List<Block>();

        private readonly Dictionary<RSAParameters, List<Message>> _inputQueue = new Dictionary<RSAParameters, List<Message>>();
        private Block _myBlock;
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();
        private CommandService _commandService;

        internal General(Decisions decision, CommandService commandService, Blockchain currentChain)
        {
            this.Decision = decision;
            this.PublicKey = _rSA.ExportParameters(false);
            this.MessageChain = new Blockchain(currentChain);

            _commandService = commandService;
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

            foreach (var general in _commandService.GetOtherGenerals(this.PublicKey))
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {

                    MessageOut message = new MessageOut(this.Decision, general.PublicKey);
                    publicDecisions.Add(message);
                }
            }

            Message broadCastMessage = Message.CreateNewMessage(inputs, publicDecisions, this);

            _commandService.BroadCastDecision(broadCastMessage, this.PublicKey);
        }


        public void NotifyBlockMined(Messenger messenger)
        {
            Block block = messenger.MinedBlock;
            if (this.MessageChain.IsValidBlock(block))
            {
                this.RecievedBlockPool.Add(block);
                this.MessageChain.Add(block);
            }
            else
            {
                this.OrphanedBlockPool.Add(block);
            }
        }

        public void RecieveMessage(Messenger messenger)
        {
            Message message = messenger.Message;
            if (this.MessageChain.IsValidMessage(message))
            {
                this.RecievedMessagePool.Add(message);
            }
            else
            {
                this.OrphanedMessagePool.Add(message);
            }
        }

        private void FinishedMiningBlock(Block block)
        {
            this.MessageChain.Add(block);
            _commandService.NotifyNewBlockMined(block, this.PublicKey);
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

        public static (RSAParameters FullKey, RSAParameters PublicKey) GenerateRSAKey()
        {
            RSACryptoServiceProvider rSA = new RSACryptoServiceProvider();
            return (rSA.ExportParameters(true), rSA.ExportParameters(false));
        }
    }
}
