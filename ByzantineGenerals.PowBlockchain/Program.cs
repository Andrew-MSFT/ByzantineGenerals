using System;
using System.Collections.Generic;

namespace ByzantineGenerals.PowBlockchain
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandService commandService = new CommandService();
            commandService.CreateGeneral(Decisions.Attack);
            commandService.CreateGeneral(Decisions.Attack);
            commandService.CreateGeneral(Decisions.Attack);
            commandService.CreateGeneral(Decisions.Retreat);
            commandService.CreateGeneral(Decisions.Retreat);

            foreach (General general in commandService.GetAllGenerals())
            {
                general.DeclareIninitialPreference();
            }

            foreach (General general in commandService.GetAllGenerals())
            {
                general.Coordinate();
            }

            Console.WriteLine("Hello World!");
        }
    }
}
