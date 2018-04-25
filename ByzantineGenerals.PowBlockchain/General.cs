using ByzantineGenerals.Lib;
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
        private List<Transaction> _inputQueue = new List<Transaction>();
        private Block _myBlock;
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();
        private MessageService _messageService;

        public General(Decisions decision, MessageService messageService)
        {
            this.Decision = decision;
            this.PublicKey = _rSA.ExportParameters(false);
            _messageService = messageService;
        }

        public void Coordinate()
        {
            InitializeBlockchain();
            CreateBaseDecision();

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
            Transaction tx = new Transaction { Input = input, Outputs = publicDecisions };

            _messageService.BroadCastDecision(tx, this.PublicKey);
        }

        private void InitializeBlockchain()
        {
            List<General> generals = _messageService.GetOtherGenerals(this.PublicKey);

            Dictionary<byte[], int> versionCount = new Dictionary<byte[], int>();
            for (int i = 0; i < generals.Count; i++)
            {
                General general = generals[i];
                if (general.MessageChain != null)
                {
                    byte[] chainHash = general.MessageChain.GetChainHash();
                    if (versionCount.ContainsKey(chainHash))
                    {
                        versionCount[chainHash]++;
                    }
                    else
                    {
                        versionCount.Add(chainHash, 1);
                    }
                }
            }

            if(versionCount.Keys.Count == 0)
            {
                this.MessageChain = new Blockchain();
            }
            else if(versionCount.Keys.Count == 1)
            {
                this.MessageChain = new Blockchain(generals[0].MessageChain.GetBlocks());
            }
        }

        private void CreateBaseDecision()
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
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            List<Transaction> transactions = new List<Transaction> { new Transaction { Input = baseMessageIn, Outputs = messageOuts } };
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
            this.MessageChain.Add(block);
        }

        public void RecieveMessage(BlockchainMessenger messenger)
        {
            Transaction message = messenger.Message;
            _inputQueue.Add(message);
            if (_inputQueue.Count == _messageService.GetOtherGenerals(this.PublicKey).Count)
            {
                MineBlock();
            }
        }

        private void MineBlock()
        {
            byte[] previousHash = MessageChain.LastBlock.ComputeSHA256();
            Block block = Block.MineNewBlock(_inputQueue, previousHash);

            _inputQueue.Clear();
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
