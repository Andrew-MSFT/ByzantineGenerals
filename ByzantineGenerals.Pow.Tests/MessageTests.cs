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

        [TestMethod]
        public void ValidBaseInputMessage()
        {
            const Decisions workingDecision = Decisions.Attack;
            CommandService commandService = new CommandService();
            General general1 = commandService.CreateGeneral(workingDecision);
            General general2 = commandService.CreateGeneral(workingDecision);

            general1.DeclareIninitialPreference();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];
            bool isValid = general2.MessageChain.IsValidMessage(message);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void InvalidBaseInputMessage()
        {
            const Decisions workingDecision = Decisions.Attack;
            CommandService commandService = new CommandService();
            General general1 = commandService.CreateGeneral(workingDecision);
            General general2 = commandService.CreateGeneral(workingDecision);

            general1.DeclareIninitialPreference();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];

            byte[] general1TargetHash = HashUtilities.ComputeSHA256(general1.PublicKey);
            byte[] general2TargetHash = HashUtilities.ComputeSHA256(general2.PublicKey);

            MessageOut input = new MessageOut
            {
                Decision = Decisions.Retreat,
                RecipientKeyHash = general1TargetHash
            };
            MessageOut output = new MessageOut
            {
                Decision = Decisions.Retreat,
                RecipientKeyHash = general2TargetHash
            };

            Message fakeMessage = Message.CreateNewMessage(new List<MessageOut> { input }, new List<MessageOut> { output }, general1);

            bool isValid = general2.MessageChain.IsValidMessage(fakeMessage);

            Assert.IsFalse(isValid);
        }
    }
}
