using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class GeneralTests
    {
        byte[] _sampleBytes = Encoding.ASCII.GetBytes("Hello World");

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
            bool isValidSignature = General.VerifySignature(general.PublicKey, message.ComputeSHA256(), signed);

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
            bool isValidSignature = General.VerifySignature(general2.PublicKey, message.ComputeSHA256(), signed);

            Assert.IsFalse(isValidSignature);
        }

        [TestMethod]
        public void MismatchedMessage()
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
            bool isValidSignature = General.VerifySignature(general1.PublicKey, message2SHA, signature);

            Assert.IsFalse(isValidSignature);
        }
    }
}
