using System;
using System.Collections.Generic;
using System.Text;

namespace ByzantineGenerals.Lib
{
    public enum Decisions { NoneRecieved, Attack, Retreat }

    public interface IGeneral
    {
        Guid Id { get; }
        Decisions Decision { get; }
        void RecieveMessage(Messenger message);
        void Coordinate();
    }
}
