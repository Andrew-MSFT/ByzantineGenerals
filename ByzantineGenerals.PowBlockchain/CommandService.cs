using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace ByzantineGenerals.PowBlockchain
{
    public class Messenger
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

    public class CommandService
    {
        private List<General> Generals { get; set; } = new List<General>();
        public static readonly Blockchain BaseBlockChain = new Blockchain();

        public CommandService() { }

        public void AddGeneral(General general)
        {
            this.Generals.Add(general);
        }

        public General CreateGeneral(Decisions decision, bool isTraitor = false)
        {
            General general = new General(decision, this, BaseBlockChain);
            this.Generals.Add(general);

            return general;
        }

        private Messenger GetMessenger()
        {
            return new Messenger();
        }

        public List<General> GetAllGenerals()
        {
            return this.Generals;
        }

        public List<General> GetOtherGenerals(RSAParameters publicKey) 
        {
            return this.Generals.Where(general => !general.PublicKey.Equals(publicKey)).ToList();
        }

        public void BroadCastDecision(Message message, RSAParameters publicKey)
        {
            List<General> generalsToNotify = GetOtherGenerals(publicKey);
            Messenger messenger = this.GetMessenger();
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
                Messenger messenger = this.GetMessenger();
                messenger.SetBlock(block);
                general.NotifyBlockMined(messenger);
            }
        }
    }
}
