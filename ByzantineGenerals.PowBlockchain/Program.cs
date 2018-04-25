using ByzantineGenerals.Lib;
using System;
using System.Collections.Generic;

namespace ByzantineGenerals.PowBlockchain
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageService messageService = new MessageService();

            messageService.Generals = new List<General>
            {
                new General(Decisions.Attack, messageService),
                new General(Decisions.Attack, messageService),
                new General(Decisions.Attack, messageService),
                new General(Decisions.Retreat, messageService),
                new General(Decisions.Retreat, messageService)
            };

            
            
            foreach (General general in messageService.Generals)
            {
                general.Coordinate();
            }

            Console.WriteLine("Hello World!");
        }
    }
}
