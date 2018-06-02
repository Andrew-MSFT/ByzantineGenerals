using ByzantineGenerals.PowBlockchain;
using System.Security.Cryptography;

namespace ByzantineGenerals.Pow.Tests
{
    class TestRSAProvider : IRSACryptoProvider
    {
        public RSAParameters PublicKey { get; private set; }
        private RSACryptoServiceProvider _rSA = new RSACryptoServiceProvider();

        internal TestRSAProvider()
        {
            this.PublicKey = _rSA.ExportParameters(false);
        }


        public byte[] SignMessage(MessageOut message)
        {
            return HashUtilities.SignMessage(message, _rSA);
        }

        public static (RSAParameters FullKey, RSAParameters PublicKey) GenerateRSAKey()
        {
            RSACryptoServiceProvider rSA = new RSACryptoServiceProvider();
            return (rSA.ExportParameters(true), rSA.ExportParameters(false));
        }
    }
}
