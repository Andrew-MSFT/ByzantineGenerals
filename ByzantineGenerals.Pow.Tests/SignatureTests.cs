using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class SignatureTests
    {
        private byte[] _recipientHash = Enumerable.Repeat<byte>(0, 32).ToArray();

        [TestMethod]
        public void MessageWithInvalidSignature()
        {

            TestRSAProvider general1 = new TestRSAProvider();
            TestRSAProvider general2 = new TestRSAProvider();
            
            MessageOut message1 = new MessageOut
            {
                Decision = Decisions.Attack,
                RecipientKeyHash = _recipientHash
            };

            MessageOut message2 = new MessageOut
            {
                Decision = Decisions.Retreat,
                RecipientKeyHash = _recipientHash
            };

            var signature = general1.SignMessage(message1);
            byte[] message2SHA = message2.ComputeSHA256();
            bool isValidSignature = HashUtilities.VerifySignature(general1.PublicKey, message2SHA, signature);

            Assert.IsFalse(isValidSignature);
        }

        [TestMethod]
        public void MessageSigning()
        {

            TestRSAProvider signer = new TestRSAProvider();
            MessageOut message = new MessageOut
            {
                Decision = Decisions.Attack,
                RecipientKeyHash = _recipientHash
            };
            var signed = signer.SignMessage(message);
            bool isValidSignature = HashUtilities.VerifySignature(signer.PublicKey, message.ComputeSHA256(), signed);

            Assert.IsTrue(isValidSignature);
        }

        [TestMethod]
        public void MismatchedKeys()
        {

            TestRSAProvider signer1 = new TestRSAProvider();
            TestRSAProvider signer2 = new TestRSAProvider();

            MessageOut message = new MessageOut
            {
                Decision = Decisions.Attack,
                RecipientKeyHash = _recipientHash
            };
            var signed = signer1.SignMessage(message);
            bool isValidSignature = HashUtilities.VerifySignature(signer2.PublicKey, message.ComputeSHA256(), signed);

            Assert.IsFalse(isValidSignature);
        }
    }
}
