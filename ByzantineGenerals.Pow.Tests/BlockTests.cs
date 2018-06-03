using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class BlockTests
    {

        [TestMethod]
        public void ContainsTransaction()
        {
            RSAParameters publicKey = TestRSAProvider.GenerateRSAKey().PublicKey;
            Message baseMessage = Message.CreateDecisionBase(Decisions.Attack, publicKey);
            Block newBlock = Block.MineNewBlock(new List<Message> { baseMessage }, Blockchain.GenesisBlockHash);

            bool containsOutput = newBlock.ContainsMessageOut(newBlock.Messages[0].Outputs[0].ComputeSHA256(), 0, out MessageOut messageOut);
            Assert.IsTrue(containsOutput);
        }

        [TestMethod]
        public void NotContainsTransaction()
        {
            RSAParameters publicKey = TestRSAProvider.GenerateRSAKey().PublicKey;
            Message baseMessage = Message.CreateDecisionBase(Decisions.Attack, publicKey);
            Block newBlock = Block.MineNewBlock(new List<Message> { baseMessage }, Blockchain.GenesisBlockHash);

            bool containsOutput = newBlock.ContainsMessageOut(newBlock.Messages[0].Outputs[0].ComputeSHA256(), 1, out MessageOut messageOut);
            Assert.IsFalse(containsOutput);
        }
    }

}
