using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
        public Decisions Decision { get; private set; }
        public byte[] RecipientKeyHash { get; private set; }

        public MessageOut(Decisions decision, RSAParameters recipientKey)
        {
            this.Decision = decision;
            this.RecipientKeyHash = HashUtilities.ComputeSHA256(recipientKey);
        }

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
            MessageOut messageOut = new MessageOut(baseMessageIn.Decision, publicKey);
            List<MessageIn> messageInputs = new List<MessageIn> { baseMessageIn };
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            Message decisionMessage = new Message { Inputs = messageInputs, Outputs = messageOuts };
            return decisionMessage;
        }

        public static Message CreateNewMessage(List<MessageOut> inputs, List<MessageOut> outputs, IGeneral general)
        {
            List<MessageIn> messageInputs = new List<MessageIn>();
            foreach (MessageOut inputMessage in inputs)
            {
                byte[] signature = general.SignMessage(inputMessage);
                MessageIn messageIn = new MessageIn
                {
                    Decision = inputMessage.Decision,
                    PreviousMessageIdx = 0,
                    PreviousMessageHash = inputMessage.CalculateSHA256(),
                    PublicKey = general.PublicKey,
                    Signature = signature
                };

                messageInputs.Add(messageIn);
            }

            Message message = new Message { Inputs = messageInputs, Outputs = outputs };
            return message;
        }

        public static bool InputMatchesOutput(MessageOut previous, MessageIn input)
        {
            //Make sure the hash of the previous matches the claimed hash in the input
            byte[] previousHash = previous.CalculateSHA256();
            bool previousMatches = previousHash.SequenceEqual(input.PreviousMessageHash);

            //Make sure that the recipient's address in the previous matches the hashed public key of the input
            byte[] inputPubKeyHash = HashUtilities.ComputeSHA256(input.PublicKey);
            bool publicKeyMatchesRecipient = previous.RecipientKeyHash.SequenceEqual(inputPubKeyHash);

            //Make sure that the signature matches the provided public key
            RSAParameters publicKey = input.PublicKey;
            bool signatureIsValid = General.VerifySignature(publicKey, previousHash, input.Signature);

            //Make sure that the amount being used matches the corresponding output value from the previous
            bool decisionsMatch = previous.Decision == input.Decision;

            return previousMatches && publicKeyMatchesRecipient && signatureIsValid && decisionsMatch;
        }
    }
}
