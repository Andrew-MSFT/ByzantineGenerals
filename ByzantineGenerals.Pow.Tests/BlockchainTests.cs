using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class BlockchainTests
    {

        [TestMethod]
        public void SimpleBlockValidation()
        {
            RSAParameters publicKey = TestRSAProvider.GenerateRSAKey().PublicKey;
            Blockchain blockchain = new Blockchain();
            Message baseMessage = Message.CreateDecisionBase(Decisions.Attack, publicKey);
            Block newBlock = Block.MineNewBlock(new List<Message> { baseMessage }, blockchain.LastBlock.ComputeSHA256());

            bool isValidBlock = blockchain.IsValidBlock(newBlock);
            Assert.IsTrue(isValidBlock);
        }

        [TestMethod]
        public void BlockValidation()
        {
            CommandService commandService = new CommandService();
            TestRSAProvider rsaProvider = new TestRSAProvider();
            Blockchain blockchain = new Blockchain();
            Message baseMessage = Message.CreateDecisionBase(Decisions.Attack, rsaProvider.PublicKey);
            blockchain.MineNextBlock(new List<Message> { baseMessage });

            List<MessageOut> newOutputs = new List<MessageOut> { new MessageOut(Decisions.Attack, rsaProvider.PublicKey) };
            Message nextMessage = Message.CreateNewMessage(baseMessage.Outputs, newOutputs, rsaProvider);
            Block nextBlock = Block.MineNewBlock(new List<Message> { nextMessage }, blockchain.LastBlock.ComputeSHA256());

            bool isValidBlock = blockchain.IsValidBlock(nextBlock);
            Assert.IsTrue(isValidBlock);
        }

        [TestMethod]
        public void InvalidBlock()
        {
            CommandService commandService = new CommandService();
            Blockchain blockchain = new Blockchain();
            TestRSAProvider rsaProvider = new TestRSAProvider();

            Message baseMessage = Message.CreateDecisionBase(Decisions.Attack, rsaProvider.PublicKey);
            blockchain.MineNextBlock(new List<Message> { baseMessage });

            List<MessageOut> newOutputs = new List<MessageOut> { new MessageOut(Decisions.Retreat, rsaProvider.PublicKey) };
            Message nextMessage = Message.CreateNewMessage(baseMessage.Outputs, newOutputs, rsaProvider);
            Block nextBlock = Block.MineNewBlock(new List<Message> { nextMessage }, blockchain.LastBlock.ComputeSHA256());

            bool isValidBlock = blockchain.IsValidBlock(nextBlock);
            Assert.IsFalse(isValidBlock);
        }

        [TestMethod]
        public void UseRecievedDecisions()
        {
            Blockchain blockchain = new Blockchain();
            TestRSAProvider rsaProvider = new TestRSAProvider();
            TestRSAProvider rsaProvider1 = new TestRSAProvider();
            Message decisionBase = Message.CreateDecisionBase(Decisions.Attack, rsaProvider.PublicKey);
            blockchain.MineNextBlock(new List<Message> { decisionBase });


            throw new NotImplementedException();
        }

    }
}
