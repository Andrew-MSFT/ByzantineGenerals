using System;
using System.Collections.Generic;

namespace ByzantineGenerals.PowBlockchain
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandService.CreateGeneral(Decisions.Attack);
            CommandService.CreateGeneral(Decisions.Attack);
            CommandService.CreateGeneral(Decisions.Attack);
            CommandService.CreateGeneral(Decisions.Retreat);
            CommandService.CreateGeneral(Decisions.Retreat);

            foreach (General general in CommandService.GetGenerals())
            {
                general.DeclareIninitialPreference();
            }

            foreach (General general in CommandService.GetGenerals())
            {
                general.Coordinate();
            }

            Console.WriteLine("Hello World!");
        }
    }
}
