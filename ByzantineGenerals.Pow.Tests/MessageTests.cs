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
            (RSAParameters FullKey, RSAParameters PublicKey) keys = General.GenerateRSAKey();
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
            CommandService commandService = new CommandService();
            General general = commandService.CreateGeneral(workingDecision);
            Message baseMessage = Message.CreateBaseDecision(workingDecision, general.PublicKey);
            MessageOut messageOut = new MessageOut(workingDecision, general.PublicKey);
            Message newMessage = Message.CreateNewMessage(baseMessage.Outputs, new List<MessageOut> { messageOut }, general);
            bool match = Message.InputMatchesOutput(baseMessage.Outputs[0], newMessage.Inputs[0]);

            Assert.IsTrue(match);
        }
    }
}
