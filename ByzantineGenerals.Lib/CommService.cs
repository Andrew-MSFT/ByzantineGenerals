using System;
using System.Collections.Generic;

namespace ByzantineGenerals.Lib
{
    public class CommService
    {
        public const Decisions DefaultDecision = Decisions.Retreat;

        public IEnumerable<IGeneral> Generals { get; private set; }

        Dictionary<Guid, int> _traitorousMessengers = new Dictionary<Guid, int>();

        public Messenger GetMessenger(Guid id)
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

        public void AssignTraitorousMessenger(Guid generalId, int count)
        {
            _traitorousMessengers.Add(generalId, count);
        }

    }
}
