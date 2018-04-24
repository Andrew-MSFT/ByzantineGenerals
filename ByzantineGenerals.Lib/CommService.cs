using System;
using System.Collections.Generic;

namespace ByzantineGenerals.Lib
{
    public class CommService
    {
        public const Decisions DefaultDecision = Decisions.Retreat;

        public IEnumerable<IGeneral> Generals { get; private set; }

        Dictionary<object, int> _traitorousMessengers = new Dictionary<object, int>();

        public Messenger GetMessenger(object id)
        {
            bool getsTraitor = _traitorousMessengers.TryGetValue(id, out int remainingTraitors);

            if (getsTraitor && remainingTraitors > 0)
            {
                _traitorousMessengers[id]--;

                return new Messenger(true);
            }

            return new Messenger(false);
        }

        public void SetGenerals(IEnumerable<IGeneral> generals)
        {
            this.Generals = generals;
        }

        public void AssignTraitorousMessenger(object generalId, int count)
        {
            _traitorousMessengers.Add(generalId, count);
        }

    }
}
