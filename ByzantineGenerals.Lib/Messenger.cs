using System;
using System.Collections.Generic;
using System.Text;

namespace ByzantineGenerals.Lib
{
    public class Message
    {
        public Guid Sender { get; set; }
        public Decisions Decision { get; set; }

        public Message(Decisions decision, Guid sender)
        {
            Sender = sender;
            Decision = decision;
        }
    }

    public class Messenger
    {
        public Message Message { get; private set; }

        private bool _isTraitor;

        public Messenger(bool isTraitor)
        {
            _isTraitor = isTraitor;
        }

        public void SetMessage(Message message)
        {
            //Change the message if a traitor
            if (_isTraitor)
            {
                if (message.Decision == Decisions.Attack)
                {
                    message.Decision = Decisions.Retreat;
                }
                else
                {
                    message.Decision = Decisions.Attack;
                }
            }

            this.Message = message;
        }
    }
}
