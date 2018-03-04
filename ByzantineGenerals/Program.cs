using ByzantineGenerals.Lib;
using System;
using System.Collections.Generic;

namespace ByzantineGenerals
{
    class Program
    {
        static void Main(string[] args)
        {
            CommService communicationService = new CommService();

            List<IGeneral> generals = new List<IGeneral> {
                new General(Decisions.Attack, communicationService),
                new General(Decisions.Attack, communicationService),
                new General(Decisions.Retreat, communicationService),
                new General(Decisions.Retreat, communicationService),
                new General(Decisions.Retreat, communicationService)
            };

            communicationService.SetGenerals(generals);
            communicationService.AssignTraitorousMessenger(generals[4].Id, 1);

            foreach(IGeneral general in generals)
            {
                general.Coordinate();
            }

            bool unamimousDecisionReached = true;
            Decisions initialDecision = generals[0].Decision;

            for (int i = 1; i < generals.Count; i++)
            {
                IGeneral general = generals[i];
                if(general.Decision != initialDecision)
                {
                    unamimousDecisionReached = false;
                    break;
                }
            } 

            if (unamimousDecisionReached)
            {
                Console.WriteLine($"Success, agreement reached.  All generals will {initialDecision}");
            }
            else
            {
                Console.Error.WriteLine("Failure, partial attack");
            }
            
        }
    }

    
}
