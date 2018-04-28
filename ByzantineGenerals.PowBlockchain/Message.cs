using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    public enum Decisions { NoneRecieved, Attack, Retreat }

    public struct MessageIn
    {
        public byte[] PreviousMessageHash { get; set; }
        public int PreviousMessageIdx { get; set; }
        public Decisions Decision { get; set; }
        public RSAParameters PublicKey { get; set; }
        public byte[] Signature { get; set; }

    }

    public struct MessageOut
    {
        public Decisions Decision { get; set; }
        public byte[] RecipientKeyHash { get; set; }

        public byte[] CalculateSHA256()
        {
            string serialized = JsonConvert.SerializeObject(this);
            byte[] thisHash = HashUtilities.ComputeSHA256(serialized);
            return thisHash;
        }
    }

    public struct Message
    {
        public List<MessageIn> Inputs { get; set; }
        public List<MessageOut> Outputs { get; set; }

        public static Message CreateBaseDecision(Decisions decision, RSAParameters publicKey)
        {

            MessageIn baseMessageIn = new MessageIn
            {
                Decision = decision,
                PreviousMessageHash = Block.DecisionInBaseHash,
                PreviousMessageIdx = Block.DecisionInBaseIndex,
                PublicKey = publicKey,
                Signature = Block.DecisionInSignature
            };
            MessageOut messageOut = new MessageOut
            {
                Decision = baseMessageIn.Decision,
                RecipientKeyHash = HashUtilities.ComputeSHA256(publicKey)
            };
            List<MessageIn> messageInputs = new List<MessageIn> { baseMessageIn };
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            Message decisionMessage = new Message { Inputs = messageInputs, Outputs = messageOuts };
            return decisionMessage;
        }
    }
}
