using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class MessageTests
    {

        [TestMethod]
        public void MessageInputsMatch()
        {
            const Decisions workingDecision = Decisions.Attack;
            IGeneral general = CommandService.CreateGeneral(workingDecision);
            Message baseMessage = Message.CreateBaseDecision(workingDecision, general.PublicKey);
            MessageOut messageOut = new MessageOut(workingDecision, general.PublicKey);
            Message newMessage = Message.CreateNewMessage(baseMessage.Outputs, new List<MessageOut> { messageOut }, general);
            bool match = Message.InputMatchesOutput(baseMessage.Outputs[0], newMessage.Inputs[0]);

            Assert.IsTrue(match);
        }
    }
}
