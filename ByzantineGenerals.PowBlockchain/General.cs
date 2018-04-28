using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ByzantineGenerals.PowBlockchain
{
    class General
    {
        public Decisions Decision { get; private set; }
        public RSAParameters PublicKey { get; private set; }
        public Blockchain MessageChain { get; private set; }
        private Dictionary<RSAParameters, List<Message>> _inputQueue = new Dictionary<RSAParameters, List<Message>>();
        private Block _myBlock;
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();
        private MessageService _messageService;

        public General(Decisions decision, MessageService messageService, Blockchain currentChain)
        {
            this.Decision = decision;
            this.PublicKey = _rSA.ExportParameters(false);
            this.MessageChain = new Blockchain(currentChain);

            _messageService = messageService;
        }

        public void DeclareIninitialPreference()
        {
            CreateBaseDecision();
        }

        public void Coordinate()
        {
            MessageIn input = CreateBaseInput();
            List<MessageOut> publicDecisions = new List<MessageOut>();

            foreach (var general in _messageService.GetOtherGenerals(this.PublicKey))
            {
                if (!general.PublicKey.Equals(this.PublicKey))
                {

                    MessageOut message = new MessageOut
                    {
                        Decision = this.Decision,
                        RecipientKeyHash = HashUtilities.ComputeSHA256(general.PublicKey)
                    };
                    publicDecisions.Add(message);
                }
            }
            List<MessageIn> inputs = new List<MessageIn> { input };
            Message tx = new Message { Input = inputs, Outputs = publicDecisions };

            _messageService.BroadCastDecision(tx, this.PublicKey);
        }

        private void CreateBaseDecision()
        {
            
            MessageIn baseMessageIn = new MessageIn
            {
                Decision = this.Decision,
                PreviousMessageHash = Block.DecisionInBaseHash,
                PreviousMessageIdx = Block.DecisionInBaseIndex,
                PublicKey = this.PublicKey,
                Signature = Block.DecisionInSignature
            };
            MessageOut messageOut = new MessageOut
            {
                Decision = this.Decision,
                RecipientKeyHash = HashUtilities.ComputeSHA256(this.PublicKey)
            };
            List<MessageIn> messageInputs = new List<MessageIn> { baseMessageIn };
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            List<Message> transactions = new List<Message> { new Message { Input = messageInputs, Outputs = messageOuts } };
            byte[] previousHash = MessageChain.LastBlock.ComputeSHA256();
            Block block = Block.MineNewBlock(transactions, previousHash);
            _myBlock = block;
            FinishedMiningBlock(block);
        }


        private MessageIn CreateBaseInput()
        {
            MessageOut inputMessage = _myBlock.Messages[0].Outputs[0];
            byte[] signature = SignMessage(inputMessage);
            MessageIn messageIn = new MessageIn
            {
                Decision = this.Decision,
                PreviousMessageIdx = 0,
                PreviousMessageHash = inputMessage.CalculateSHA256(),
                PublicKey = this.PublicKey,
                Signature = signature
            };
            return messageIn;
        }

        public void NotifyBlockMined(BlockchainMessenger messenger)
        {
            Block block = messenger.MinedBlock;
            this.MessageChain?.Add(block);
        }

        public void RecieveMessage(BlockchainMessenger messenger)
        {
            Message message = messenger.Message;
            //_inputQueue.Add(message.Input.PublicKey, message);
            if (_inputQueue.Count == _messageService.GetOtherGenerals(this.PublicKey).Count)
            {
                //MineBlock();
            }
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
            _messageService.NotifyNewBlockMined(block, this.PublicKey);
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
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(publicKey);
            RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(RSA);
            RSADeformatter.SetHashAlgorithm("SHA256");
            byte[] hashedData = HashUtilities.ComputeSHA256(originalData);
            bool signatureIsValid = RSADeformatter.VerifySignature(hashedData, signedHash);
            return signatureIsValid;
        }
    }
}
