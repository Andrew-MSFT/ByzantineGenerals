using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void BaseMessage()
        {
            const Decisions workingDecision = Decisions.Attack;
            (RSAParameters FullKey, RSAParameters PublicKey) keys = TestRSAProvider.GenerateRSAKey();
            Message baseMessage = Message.CreateBaseDecision(workingDecision, keys.PublicKey);
            byte[] pubKeyHash = HashUtilities.ComputeSHA256(keys.PublicKey);

            Assert.AreEqual(1, baseMessage.Inputs.Count);
            Assert.IsTrue(baseMessage.Inputs[0].IsBaseMessage());
            Assert.AreEqual(1, baseMessage.Outputs.Count);
            Assert.IsTrue(baseMessage.Outputs[0].RecipientKeyHash.SequenceEqual(pubKeyHash));
            Assert.AreEqual(workingDecision, baseMessage.Outputs[0].Decision);
        }

        [TestMethod]
        public void MessageInputsMatch()
        {
            const Decisions workingDecision = Decisions.Attack;
            var rsaProvider = new TestRSAProvider();

            Message baseMessage = Message.CreateBaseDecision(workingDecision, rsaProvider.PublicKey);
            MessageOut messageOut = new MessageOut(workingDecision, rsaProvider.PublicKey);
            Message newMessage = Message.CreateNewMessage(baseMessage.Outputs, new List<MessageOut> { messageOut }, rsaProvider);
            bool match = Message.InputMatchesOutput(baseMessage.Outputs[0], newMessage.Inputs[0], baseMessage.SenderPublicKey);

            Assert.IsTrue(match);
        }

        [TestMethod]
        public void OutputsAreConsistent()
        {
            var rsaProvider1 = new TestRSAProvider();
            var rsaProvider2 = new TestRSAProvider();

            Message baseMessage = Message.CreateBaseDecision(Decisions.Attack, rsaProvider1.PublicKey);
            MessageOut messageOut1 = new MessageOut(Decisions.Attack, rsaProvider1.PublicKey);
            MessageOut messageOut2 = new MessageOut(Decisions.Attack, rsaProvider2.PublicKey);
            Message newMessage = Message.CreateNewMessage(baseMessage.Outputs, new List<MessageOut> { messageOut1, messageOut2 }, rsaProvider1);

            Assert.IsTrue(Message.MessageIsConsistent(newMessage));
        }

        [TestMethod]
        public void OutputsAreInconsistent()
        {
            var rsaProvider1 = new TestRSAProvider();
            var rsaProvider2 = new TestRSAProvider();

            Message baseMessage = Message.CreateBaseDecision(Decisions.Attack, rsaProvider1.PublicKey);
            MessageOut messageOut1 = new MessageOut(Decisions.Attack, rsaProvider1.PublicKey);
            MessageOut messageOut2 = new MessageOut(Decisions.Retreat, rsaProvider2.PublicKey);
            Message newMessage = Message.CreateNewMessage(baseMessage.Outputs, new List<MessageOut> { messageOut1, messageOut2 }, rsaProvider1);

            Assert.IsFalse(Message.MessageIsConsistent(newMessage));
        }


    }
}
