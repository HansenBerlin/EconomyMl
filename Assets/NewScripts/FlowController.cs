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
        public int StartMonthBusinessDecisionsMade
        {
            get => _startMonthBusinessDecisionsMade;
            set
            {
                _startMonthBusinessDecisionsMade = value;
                if (_startMonthBusinessDecisionsMade == ServiceLocator.Instance.Companys.Count)
                {
                    Step = SimulationStep.StartMonthHouseholds;
                }
                else if (_startMonthBusinessDecisionsMade == 0)
                {
                    Step = SimulationStep.StartMonthBusiness;
                }
            } 
        }
        private int _startMonthBusinessDecisionsMade;
        
        public int StartDayBusinessDecisionsMade
        {
            get => _startDayBusinessDecisionsMade;
            set
            {
                _startDayBusinessDecisionsMade = value;
                if (_startDayBusinessDecisionsMade == ServiceLocator.Instance.Companys.Count)
                {
                    Step = SimulationStep.StartDaysHouseholds;
                }
                else if (_startDayBusinessDecisionsMade == 0)
                {
                    Step = SimulationStep.StartDaysBusiness;
                }
            } 
        }
        private int _startDayBusinessDecisionsMade;
        public SimulationStep Step = SimulationStep.StartMonthBusiness;

        public void IncrementDay()
        {
            StartDayBusinessDecisionsMade = 0;
            //Step = SimulationStep.StartDaysBusiness;
            if (Day == 20)
            {
                Day = 1;
            }
            else
            {
                Day++;
            }
        }

        public void IncrementMonth()
        {
            StartMonthBusinessDecisionsMade = 0;
            Step = SimulationStep.StartMonthBusiness;
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
            yield return new WaitUntil(()=>Step == SimulationStep.StartMonthHouseholds);
            whenDone?.Invoke();
        }
        
        public IEnumerator WaitUntilStartDaysHouseholdPhase (Action whenDone)
        {
            yield return new WaitUntil(()=>Step == SimulationStep.StartDaysHouseholds);
            whenDone?.Invoke();
        }

        public void CommitDecision()
        {
            if (Step == SimulationStep.StartMonthBusiness)
            {
                StartMonthBusinessDecisionsMade++;
            }
            else if (Step == SimulationStep.StartDaysBusiness)
            {
                StartDayBusinessDecisionsMade++;
            }
        }
    }
}