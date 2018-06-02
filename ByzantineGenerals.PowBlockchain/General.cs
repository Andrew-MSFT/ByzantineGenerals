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
    public interface IRSACryptoProvider
    {
        RSAParameters PublicKey { get; }
        byte[] SignMessage(MessageOut message);
    }

    public class General : IRSACryptoProvider
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
        private Dictionary<RSAParameters, Decisions> _currentDecisionTally = new Dictionary<RSAParameters, Decisions>();

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
            this.RecievedMessagePool.Insert(0, initialDecision);
            _myBlock = MineNewBlock();
            DecisionArrived(this.PublicKey, this.Decision);
        }

        public void DeclareCurrentDecision()
        {

        }

        private void DecisionArrived(RSAParameters generalsKey, Decisions decision)
        {
            if (_currentDecisionTally.ContainsKey(generalsKey))
            {
                _currentDecisionTally[generalsKey] = decision;
            }
            else
            {
                _currentDecisionTally.Add(generalsKey, decision);
            }
        }

        public Decisions GetCurrentConsensus()
        {
            int retreatVotes = 0;
            int attackVotes = 0;

            foreach (var decision in _currentDecisionTally.Values)
            {
                if (decision == Decisions.Attack)
                {
                    attackVotes++;
                }
                else if (decision == Decisions.Retreat)
                {
                    retreatVotes++;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            Decisions currentDecision = attackVotes > retreatVotes ? Decisions.Attack : Decisions.Retreat;
            return currentDecision;
        }

        public Block MineNewBlock()
        {
            List<Message> transactions = new List<Message>();

            //Verify messages are valid before adding them to the block
            foreach (var message in this.RecievedMessagePool)
            {
                if (this.MessageChain.IsValidMessage(message))
                {
                    transactions.Add(message);
                }
                else
                {
                    this.OrphanedMessagePool.Add(message);
                }
            }
            this.RecievedMessagePool.Clear();

            byte[] previousHash = MessageChain.LastBlock.ComputeSHA256();
            Block block = Block.MineNewBlock(transactions, previousHash);

            this.MessageChain.Add(block);
            _commandService.NotifyNewBlockMined(block, this.PublicKey);

            return block;
        }

        public void Coordinate()
        {
            List<MessageOut> publicDecisions = new List<MessageOut>();
            List<MessageOut> inputs = new List<MessageOut> { _myBlock.Messages[0].Outputs[0] };

            foreach (var general in _commandService.GetOtherGenerals(this.PublicKey))
            {
                MessageOut message = new MessageOut(this.Decision, general.PublicKey);
                publicDecisions.Add(message);
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
                //TO DO: Update incoming decision
                foreach (var message in block.Messages)
                {
                    this.RecievedMessagePool.Remove(message);
                }
            }
            else
            {
                this.OrphanedBlockPool.Add(block);
            }
        }

        public void RecieveMessage(Messenger messenger)
        {
            Message message = messenger.Message;
            this.RecievedMessagePool.Add(message);
        }

        public byte[] SignMessage(MessageOut message)
        {
            return HashUtilities.SignMessage(message, _rSA);
        }


    }
}
