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
        private List<IGeneral> Generals { get; set; } = new List<IGeneral>();
        private static CommandService _service = new CommandService();
        public static Blockchain BaseBlockChain = new Blockchain();

        private CommandService() { }

        public static void AddGeneral(IGeneral general)
        {
            _service.Generals.Add(general);
        }

        public static IGeneral CreateGeneral(Decisions decision, bool isTraitor = false)
        {
            IGeneral general = new General(decision, BaseBlockChain);
            _service.Generals.Add(general);

            return general;
        }

        private Messenger GetMessenger()
        {
            return new Messenger();
        }

        public static List<IGeneral> GetGenerals()
        {
            return _service.Generals;
        }

        public static List<IGeneral> GetOtherGenerals(RSAParameters publicKey) 
        {
            return _service.Generals.Where(general => !general.PublicKey.Equals(publicKey)).ToList();
        }

        public static void BroadCastDecision(Message message, RSAParameters publicKey)
        {
            List<IGeneral> generalsToNotify = CommandService.GetOtherGenerals(publicKey);
            Messenger messenger = _service.GetMessenger();
            messenger.SetMessage(message);

            foreach (IGeneral general in generalsToNotify)
            {
                Debug.Assert(!general.PublicKey.Equals(publicKey));
                general.RecieveMessage(messenger);
            }
        }

        public static void NotifyNewBlockMined(Block block, RSAParameters publicKey)
        {
            List<IGeneral> generalsToNotify = GetOtherGenerals(publicKey);

            foreach (IGeneral general in generalsToNotify)
            {
                Debug.Assert(!general.PublicKey.Equals(publicKey));
                Messenger messenger = _service.GetMessenger();
                messenger.SetBlock(block);
                general.NotifyBlockMined(messenger);
            }
        }
    }
}
