using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewScripts
{
    public class FlowController
    {
        public int Year { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int Day { get; set; } = 1;
        public int StartMonthBusinessDecisionsMade { get; set; }
        
        public int StartMonthHouseholdDecisionMade
        {
            get => _startMonthHouseholdDecisionMade;
            set
            {
                _startMonthHouseholdDecisionMade = value;
                if (_startMonthHouseholdDecisionMade == ServiceLocator.Instance.LaborMarketService.Workers.Count)
                {
                    _startMonthHouseholdDecisionMade = 0;
                }
            } 
        }
        private int _startMonthHouseholdDecisionMade;
        //public SimulationStep Step = SimulationStep.StartMonthBusiness;

        public void IncrementDay()
        {
            Day++;
            if (Day == 21)
            {
                Day = 1;
                Month++;
            }
            if (Month == 13)
            {
                Month = 1;
                Year++;
            }
        }

        public void IncrementMonth()
        {
            //StartMonthBusinessDecisionsMade = 0;
            //Step = SimulationStep.StartMonthBusiness;
            if (Month == 12)
            {
                Year++;
                Month = 1;
            }
            else
            {
                Month++;
            }
        }

        public void Reset()
        {
            Year = 1;
            Month = 1;
            Day = 1;
        }

        public string Current()
        {
            return $"{Day}.{Month}.{Year}";
        }
        
        public IEnumerator WaitUntilStartMonthHouseholdPhase (Action whenDone)
        {
            yield return new WaitUntil(()=>StartMonthBusinessDecisionsMade == ServiceLocator.Instance.Companys.Count);
            whenDone?.Invoke();
        }
        
        public IEnumerator WaitUntilStartDaysPhase (Action whenDone)
        {
            yield return new WaitUntil(()=>StartMonthBusinessDecisionsMade == ServiceLocator.Instance.Companys.Count);
            whenDone?.Invoke();
        }
        

        public void CommitDecision()
        {
            StartMonthBusinessDecisionsMade++;
        }
        
        public void ResetCounter()
        {
            StartMonthBusinessDecisionsMade = 0;
        }
    }
}