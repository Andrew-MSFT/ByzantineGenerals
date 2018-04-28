using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    class BlockchainMessenger
    {
        public Message Message { get; private set; }
        public Block MinedBlock { get; private set; }

        public void SetMessage(Message transaction)
        {
            this.Message = transaction;
        }

        public void SetBlock(Block block)
        {
            this.MinedBlock = block;
        }
    }

    class MessageService
    {
        public List<General> Generals { get; set; }

        public BlockchainMessenger GetMessenger()
        {
            return new BlockchainMessenger();
        }

        public List<General> GetOtherGenerals(RSAParameters publicKey) 
        {
            return this.Generals.Where(general => !general.PublicKey.Equals(publicKey)).ToList();
        }

        public void BroadCastDecision(Message message, RSAParameters publicKey)
        {
            List<General> generalsToNotify = GetOtherGenerals(publicKey);
            BlockchainMessenger messenger = GetMessenger();
            messenger.SetMessage(message);

            foreach (General general in generalsToNotify)
            {
                Debug.Assert(!general.PublicKey.Equals(publicKey));
                general.RecieveMessage(messenger);
            }
        }

        public void NotifyNewBlockMined(Block block, RSAParameters publicKey)
        {
            List<General> generalsToNotify = GetOtherGenerals(publicKey);

            foreach (General general in generalsToNotify)
            {
                Debug.Assert(!general.PublicKey.Equals(publicKey));
                BlockchainMessenger messenger = GetMessenger();
                messenger.SetBlock(block);
                general.NotifyBlockMined(messenger);
            }
        }
    }
}
