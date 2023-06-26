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
        //public int Day { get; set; } = 1;
        public int StartMonthBusinessDecisionsMade { get; set; }
        
        public int StartMonthHouseholdDecisionMade
        {
            get => _startMonthHouseholdDecisionMade;
            set
            {
                _startMonthHouseholdDecisionMade = value;
                if (_startMonthHouseholdDecisionMade == ServiceLocator.Instance.LaborMarket.Workers.Count)
                {
                    _startMonthHouseholdDecisionMade = 0;
                }
            } 
        }
        private int _startMonthHouseholdDecisionMade;
        //public SimulationStep Step = SimulationStep.StartMonthBusiness;

        public void IncrementMonth()
        {
            //Day++;
            Month++;
            //if (Day == 21)
            //{
            //    Day = 1;
            //}
            if (Month == 13)
            {
                Month = 1;
                Year++;
            }
        }

        public string Current()
        {
            return $"{Month}.{Year}";
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
        
        public bool Proceed()
        {
            if (StartMonthBusinessDecisionsMade == ServiceLocator.Instance.Companys.Count)
            {
                StartMonthBusinessDecisionsMade = 0;
                return true;
                //IncrementMonth();
            }

            return false;
        }
    }
}