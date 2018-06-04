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

            general1.MakeBaseDeclaration();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];
            bool isValid = general2.MessageChain.IsValidMessage(message);

            Assert.IsTrue(isValid);
        }

       [TestMethod]
        public void ValidateSenderBlockChain()
        {
            CommandService commandService = new CommandService();
            General general = commandService.CreateGeneral(Decisions.Attack);
            General testGeneral = commandService.CreateGeneral(Decisions.Attack);

            general.MakeBaseDeclaration();
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

            general1.MakeBaseDeclaration();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];

            MessageOut input = message.Outputs[0];
            MessageOut output = new MessageOut
            {
                Decision = input.Decision,
                RecipientKeyHash = general2.PublicKeyHash
            };

            Message fakeMessage = Message.CreateNewMessage(new List<MessageOut> { input }, general2.PublicKey, general1);
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

            general1.MakeBaseDeclaration();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];

            MessageOut input = message.Outputs[0];
            MessageOut output = new MessageOut
            {
                Decision = input.Decision,
                RecipientKeyHash = general2.PublicKeyHash
            };

            Message validMessage = Message.CreateNewMessage(new List<MessageOut> { input }, general2.PublicKey, general1);
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

            general1.MakeBaseDeclaration();
            Block newBlock = general2.RecievedBlockPool[0];
            Message message = newBlock.Messages[0];

            MessageOut input = new MessageOut
            {
                Decision = Decisions.Retreat,
                RecipientKeyHash = general1.PublicKeyHash
            };
            MessageOut output = new MessageOut
            {
                Decision = Decisions.Retreat,
                RecipientKeyHash = general2.PublicKeyHash
            };

            Message fakeMessage = Message.CreateNewMessage(new List<MessageOut> { input }, general2.PublicKey, general1);
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

            general1.MakeBaseDeclaration();

            List<MessageOut> publicDecisions = new List<MessageOut>();
            MessageOut messageOut = general1.MessageChain[general1.MessageChain.Count - 1].Messages[0].Outputs[0];
            List<MessageOut> inputs = new List<MessageOut> { messageOut };

            MessageOut message = new MessageOut(workingDecision, general2.PublicKey);
            publicDecisions.Add(message);

            Message broadCastMessage = Message.CreateNewMessage(inputs, general2.PublicKey, general1);
            commandService.BroadCastDecision(broadCastMessage, keys.PublicKey);

            Assert.AreEqual(1, general1.RecievedMessagePool.Count);
            Assert.AreEqual(1, general2.RecievedMessagePool.Count);

            general2.MineNewBlock();

            Assert.AreEqual(0, general1.RecievedMessagePool.Count);
            Assert.AreEqual(0, general2.RecievedMessagePool.Count);
        }
    }
}
