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

            bool containsOutput = newBlock.ContainsOutTransaction(newBlock.Messages[0].Outputs[0].CalculateSHA256(), out MessageOut messageOut);
            Assert.IsTrue(containsOutput);
            Assert.IsNotNull(messageOut);
        }

        [TestMethod]
        public void ValidateMessage()
        {
            General general = CommandService.CreateGeneral(Decisions.Attack);
        }
    }
}
