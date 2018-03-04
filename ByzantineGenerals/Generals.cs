using ByzantineGenerals.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByzantineGenerals
{


    class General : IGeneral
    {
        public Guid Id { get; private set; }
        public Decisions Decision { get; protected set; }

        int _attackCount = 0;
        int _retreatCount = 0;
        Decisions _preferredDecision;
        protected CommService _comService;

        public General(Decisions preferredDecision, CommService comService)
        {
            this.Id = Guid.NewGuid();

            _preferredDecision = preferredDecision;
            _comService = comService;

            IncrementCounts(preferredDecision);
        }

        public void RecieveMessage(Messenger messenger)
        {
            Message message = messenger.Message;
            IncrementCounts(message.Decision);
        }

        public void Coordinate()
        {
            foreach(IGeneral general in _comService.Generals)
            {
                if (general.Id != this.Id)
                {
                    Messenger messenger = _comService.GetMessenger(this.Id);
                    Message message = new Message(_preferredDecision, this.Id);
                    messenger.SetMessage(message);
                    general.RecieveMessage(messenger);
                }
            }
        }

        private void IncrementCounts(Decisions incomingDecision)
        {
            if (incomingDecision == Decisions.Attack)
            {
                _attackCount++;
            }
            else
            {
                _retreatCount++;
            }

            //Default decision is to retreat, so the Attacks must be a majority to change this
            if(_attackCount > _retreatCount)
            {
                Decision = Decisions.Attack;
            }
            else
            {
                Decision = Decisions.Retreat;
            }
        }
    }

    class TraitorGeneral : IGeneral
    {
        public Guid Id { get; private set; }
        public Decisions Decision { get; private set; }

        CommService _comService;

        public TraitorGeneral(CommService comService)
        {
            this.Id = Guid.NewGuid();

            _comService = comService;
        }

        public void RecieveMessage(Messenger messenger)
        {
            Guid senderId = messenger.Message.Sender;
            IGeneral sender = _comService.Generals.Where(general => general.Id == senderId).FirstOrDefault();
            Message message = new Message(messenger.Message.Decision, this.Id);
            messenger.SetMessage(message);
            sender.RecieveMessage(messenger);
        }

        public void Coordinate()
        {
            //Do nothing just wait and confirm other generals decisions
        }
    }
}
