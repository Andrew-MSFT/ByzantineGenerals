using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class BlockTests
    {
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();

        [TestMethod]
        public void ContainsTransaction()
        {
            RSAParameters publicKey = _rSA.ExportParameters(false);
            Blockchain blockchain = new Blockchain();
            Message baseMessage = Message.CreateBaseDecision(Decisions.Attack, publicKey);
            Block newBlock = Block.MineNewBlock(new List<Message> { baseMessage }, blockchain.LastBlock.ComputeSHA256());

            bool containsOutput = newBlock.ContainsMessageOut(newBlock.Messages[0].Outputs[0].CalculateSHA256(), out MessageOut messageOut);
            Assert.IsTrue(containsOutput);
            Assert.IsNotNull(messageOut);
        }

        [TestMethod]
        public void ValidateMessage()
        {
            General general = (General)CommandService.CreateGeneral(Decisions.Attack);
            TestGeneral testGeneral = new TestGeneral(Decisions.Attack);
            CommandService.AddGeneral(testGeneral);

            general.DeclareIninitialPreference();
            Block recievedBlock = testGeneral.ReceivedBlocks[0];
            bool blockInChain = general.MessageChain.ContainsBlock(recievedBlock);
            
            Assert.IsTrue(blockInChain);
        }
    }

    class TestGeneral : IGeneral
    {
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();

        public RSAParameters PublicKey { get; private set; }

        public Decisions Decision { get; private set; }

        public Blockchain MessageChain { get; private set; }

        public List<Message> RecievedMessages { get; private set; } = new List<Message>();
        public List<Block> ReceivedBlocks { get; private set; } = new List<Block>();

        public TestGeneral(Decisions decision)
        {
            this.Decision = decision;
            this.PublicKey = _rSA.ExportParameters(false);
            this.MessageChain = new Blockchain(CommandService.BaseBlockChain);
        }

        public void NotifyBlockMined(Messenger messenger)
        {
            this.ReceivedBlocks.Add(messenger.MinedBlock);
        }

        public void RecieveMessage(Messenger messenger)
        {
            this.RecievedMessages.Add(messenger.Message);
        }

        public byte[] SignMessage(MessageOut message)
        {
            throw new System.NotImplementedException();
        }
    }
}
