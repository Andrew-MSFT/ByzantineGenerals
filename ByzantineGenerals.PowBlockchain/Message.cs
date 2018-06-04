using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    public enum Decisions { Attack, Retreat }

    public struct MessageIn
    {
        public byte[] PreviousMessageHash { get; set; }
        public int PreviousMessageBlockIdx { get; set; }
        public int PreviousMessageIdx { get; set; }
        public Decisions Decision { get; set; }
        public byte[] Signature { get; set; }

        public MessageIn(byte[] previousMessageHash, int previousMessageBlockIdx, int previousMessageIdx, Decisions decision,
            byte[] signature)
        {
            this.PreviousMessageHash = previousMessageHash;
            this.PreviousMessageBlockIdx = previousMessageBlockIdx;
            this.PreviousMessageIdx = previousMessageIdx;
            this.Decision = decision;
            this.Signature = signature;
        }

        public bool IsBaseMessage()
        {
            return this.PreviousMessageHash.SequenceEqual(Block.DecisionInBaseHash) &&
                this.PreviousMessageIdx == Block.DecisionInBaseIndex &&
                this.Signature.SequenceEqual(Block.DecisionInSignature);
        }

    }

    public struct MessageOut
    {
        public Decisions Decision { get; set; }
        public byte[] RecipientKeyHash { get; set; }
        public Guid Id { get; set; }

        public MessageOut(Decisions decision, RSAParameters recipientKey)
        {
            this.Decision = decision;
            this.RecipientKeyHash = HashUtilities.ComputeSHA256(recipientKey);
            this.Id = Guid.NewGuid();
        }

        public byte[] ComputeSHA256()
        {
            string serialized = JsonConvert.SerializeObject(this);
            byte[] thisHash = HashUtilities.ComputeSHA256(serialized);
            return thisHash;
        }
    }

    public struct Message
    {
        public RSAParameters SenderPublicKey { get; set; }
        public List<MessageIn> Inputs { get; set; }
        public List<MessageOut> Outputs { get; set; }

        public Decisions GetInputConsensus()
        {
            return GetConsensusDecision(this.Inputs);
        }

        public static Decisions GetConsensusDecision(List<MessageIn> inputs)
        {
            int attackCount = 0;
            int retreatCount = 0;

            foreach (var input in inputs)
            {
                if (input.Decision == Decisions.Attack)
                {
                    attackCount++;
                }
                else if (input.Decision == Decisions.Retreat)
                {
                    retreatCount++;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return attackCount > retreatCount ? Decisions.Attack : Decisions.Retreat;
        }

        public static Message CreateDecisionBase(Decisions decision, RSAParameters publicKey)
        {

            MessageIn baseMessageIn = new MessageIn(
                Block.DecisionInBaseHash,
                Block.DecisionInBaseIndex,
                Block.DecisionInBaseIndex,
                decision,
                Block.DecisionInSignature
            );
            MessageOut messageOut = new MessageOut(baseMessageIn.Decision, publicKey);
            List<MessageIn> messageInputs = new List<MessageIn> { baseMessageIn };
            List<MessageOut> messageOuts = new List<MessageOut> { messageOut };
            Message decisionMessage = new Message
            {
                SenderPublicKey = publicKey,
                Inputs = messageInputs,
                Outputs = messageOuts
            };
            return decisionMessage;
        }

        public static Message CreateNewMessage(List<MessageOut> inputs, RSAParameters recipient, IRSACryptoProvider sender)
        {
            List<RSAParameters> recipients = new List<RSAParameters> { recipient };
            return CreateNewMessage(inputs, recipients, sender);
        }

        public static Message CreateNewMessage(List<MessageOut> inputs, List<MessageOut> messageOutputs, IRSACryptoProvider sender)
        {
            List<MessageIn> messageInputs = CreateMessageInputs(inputs, sender);

            Message message = new Message
            {
                SenderPublicKey = sender.PublicKey,
                Inputs = messageInputs,
                Outputs = messageOutputs
            };
            return message;
        }

            public static Message CreateNewMessage(List<MessageOut> inputs, List<RSAParameters> recipients, IRSACryptoProvider sender)
        {
            List<MessageIn> messageInputs = CreateMessageInputs(inputs, sender);
            List<MessageOut> messageOutputs = CreateMessageOutputs(recipients, messageInputs);

            Message message = new Message
            {
                SenderPublicKey = sender.PublicKey,
                Inputs = messageInputs,
                Outputs = messageOutputs
            };
            return message;
        }

        private static List<MessageOut> CreateMessageOutputs(List<RSAParameters> recipients, List<MessageIn> messageInputs)
        {
            List<MessageOut> messageOutputs = new List<MessageOut>();
            Decisions outputDecision = Message.GetConsensusDecision(messageInputs);

            foreach (RSAParameters recipient in recipients)
            {
                MessageOut messageOut = new MessageOut(outputDecision, recipient);
                messageOutputs.Add(messageOut);
            }

            return messageOutputs;
        }

        internal static List<MessageIn> CreateMessageInputs(List<MessageOut> inputs, IRSACryptoProvider sender)
        {
            List<MessageIn> messageInputs = new List<MessageIn>();
            foreach (MessageOut inputMessage in inputs)
            {
                byte[] signature = sender.SignMessage(inputMessage);
                MessageIn messageIn = new MessageIn
                (
                    inputMessage.ComputeSHA256(),
                    -1, //TO DO BlockIdx
                    0,
                    inputMessage.Decision,
                    signature
                );

                messageInputs.Add(messageIn);
            }

            return messageInputs;
        }

        public static bool InputMatchesOutput(MessageOut previous, MessageIn input, RSAParameters senderPublicKey)
        {
            //Make sure the hash of the previous matches the claimed hash in the input
            byte[] previousHash = previous.ComputeSHA256();
            bool previousMatches = previousHash.SequenceEqual(input.PreviousMessageHash);

            //Make sure that the recipient's address in the previous matches the hashed public key of the input
            byte[] inputPubKeyHash = HashUtilities.ComputeSHA256(senderPublicKey);
            bool publicKeyMatchesRecipient = previous.RecipientKeyHash.SequenceEqual(inputPubKeyHash);

            //Make sure that the signature matches the provided public key
            bool signatureIsValid = HashUtilities.VerifySignature(senderPublicKey, previousHash, input.Signature);

            //Make sure that the amount being used matches the corresponding output value from the previous
            bool decisionsMatch = previous.Decision == input.Decision;

            return previousMatches && publicKeyMatchesRecipient && signatureIsValid && decisionsMatch;
        }

        public static bool MessageIsConsistent(Message message)
        {
            Decisions inputConsensus = message.GetInputConsensus();

            foreach (var output in message.Outputs)
            {
                if (output.Decision != inputConsensus)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
