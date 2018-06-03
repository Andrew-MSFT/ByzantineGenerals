using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class GeneralTests
    {
        byte[] _sampleBytes = Encoding.ASCII.GetBytes("Hello World");

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
        public void MessageSigning()
        {

            General general = new General(Decisions.Attack, null, new Blockchain());
            var recipientHash = HashUtilities.ComputeSHA256(general.PublicKey);
            MessageOut message = new MessageOut
            {
                Decision = Decisions.Attack,
                RecipientKeyHash = recipientHash
            };
            var signed = general.SignMessage(message);
            bool isValidSignature = HashUtilities.VerifySignature(general.PublicKey, message.ComputeSHA256(), signed);

            Assert.IsTrue(isValidSignature);
        }

        [TestMethod]
        public void MismatchedKeys()
        {

            General general1 = new General(Decisions.Attack, null, new Blockchain());
            General general2 = new General(Decisions.Attack, null, new Blockchain());
            var recipientHash = HashUtilities.ComputeSHA256(general1.PublicKey);
            MessageOut message = new MessageOut
            {
                Decision = Decisions.Attack,
                RecipientKeyHash = recipientHash
            };
            var signed = general1.SignMessage(message);
            bool isValidSignature = HashUtilities.VerifySignature(general2.PublicKey, message.ComputeSHA256(), signed);

            Assert.IsFalse(isValidSignature);
        }

        [TestMethod]
        public void MessageWithInvalidSignature()
        {

            General general1 = new General(Decisions.Attack, null, new Blockchain());
            General general2 = new General(Decisions.Retreat, null, new Blockchain());
            var recipientHash = HashUtilities.ComputeSHA256(general1.PublicKey);
            MessageOut message1 = new MessageOut
            {
                Decision = general1.Decision,
                RecipientKeyHash = recipientHash
            };

            MessageOut message2 = new MessageOut
            {
                Decision = general2.Decision,
                RecipientKeyHash = recipientHash
            };

            var signature = general1.SignMessage(message1);
            byte[] message2SHA = message2.ComputeSHA256();
            bool isValidSignature = HashUtilities.VerifySignature(general1.PublicKey, message2SHA, signature);

            Assert.IsFalse(isValidSignature);
        }

        [TestMethod]
        public void ValidateSenderBlockChain()
        {
            CommandService commandService = new CommandService();
            General general = commandService.CreateGeneral(Decisions.Attack);
            General testGeneral = commandService.CreateGeneral(Decisions.Attack);

            general.DeclareIninitialPreference();
            Block recievedBlock = testGeneral.RecievedBlockPool[0];
            bool blockInChain = general.MessageChain.ContainsBlock(recievedBlock);

            Assert.IsTrue(blockInChain);
        }

        [TestMethod]
        public void SendInvalidBlock()
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

            MessageOut input = message.Outputs[0];
            MessageOut output = new MessageOut
            {
                Decision = input.Decision,
                RecipientKeyHash = general2TargetHash
            };

            Message fakeMessage = Message.CreateNewMessage(new List<MessageOut> { input }, new List<MessageOut> { output }, general1);
            bool isValid = general2.MessageChain.IsValidMessage(fakeMessage);
            commandService.BroadCastDecision(fakeMessage, general1.PublicKey);

            Assert.IsTrue(isValid);
            Assert.AreEqual(0, general2.OrphanedMessagePool.Count);
            Assert.AreEqual(1, general2.RecievedMessagePool.Count);
        }

        [TestMethod]
        public void ValidMessage()
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

            MessageOut input = message.Outputs[0];
            MessageOut output = new MessageOut
            {
                Decision = input.Decision,
                RecipientKeyHash = general2TargetHash
            };

            Message validMessage = Message.CreateNewMessage(new List<MessageOut> { input }, new List<MessageOut> { output }, general1);
            bool isValid = general2.MessageChain.IsValidMessage(validMessage);
            commandService.BroadCastDecision(validMessage, general1.PublicKey);

            Assert.IsTrue(isValid);
            Assert.AreEqual(0, general2.OrphanedMessagePool.Count);
            Assert.AreEqual(1, general2.RecievedMessagePool.Count);
        }

        [TestMethod]
        public void FakeMessage()
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
            commandService.BroadCastDecision(fakeMessage, general1.PublicKey);
            general2.MineNewBlock();

            Assert.IsFalse(isValid);
            Assert.AreEqual(1, general2.OrphanedMessagePool.Count);
            Assert.AreEqual(0, general2.RecievedMessagePool.Count);
        }

        [TestMethod]
        public void RemoveQueuedMessages()
        {
            const Decisions workingDecision = Decisions.Attack;
            CommandService commandService = new CommandService();
            var keys = TestRSAProvider.GenerateRSAKey();
            General general1 = commandService.CreateGeneral(workingDecision);
            General general2 = commandService.CreateGeneral(workingDecision);

            general1.DeclareIninitialPreference();

            List<MessageOut> publicDecisions = new List<MessageOut>();
            MessageOut messageOut = general1.MessageChain[general1.MessageChain.Count - 1].Messages[0].Outputs[0];
            List<MessageOut> inputs = new List<MessageOut> { messageOut };

            MessageOut message = new MessageOut(general1.Decision, general2.PublicKey);
            publicDecisions.Add(message);

            Message broadCastMessage = Message.CreateNewMessage(inputs, publicDecisions, general1);
            commandService.BroadCastDecision(broadCastMessage, keys.PublicKey);

            Assert.AreEqual(1, general1.RecievedMessagePool.Count);
            Assert.AreEqual(1, general2.RecievedMessagePool.Count);

            general2.MineNewBlock();

            Assert.AreEqual(0, general1.RecievedMessagePool.Count);
            Assert.AreEqual(0, general2.RecievedMessagePool.Count);
        }
    }
}
