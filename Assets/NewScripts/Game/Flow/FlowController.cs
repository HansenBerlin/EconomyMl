using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Game.Services;

namespace NewScripts.Game.Flow
{
    public class FlowController
    {
        public int Year { get; private set; } = 1;
        public int Month { get; private set; } = 1;
        private Dictionary<int, CompanyDecisionStatus> DecisionStati { get; set; } = new();
        public bool IsGovernmentDecisionCommitted { get; set; } = true;

        public FlowController(List<int> companyIds)
        {
            foreach (var companyId in companyIds)
            {
                DecisionStati.Add(companyId, CompanyDecisionStatus.Requested);
            }
        }

        public void IncrementMonth()
        {
            Month++;
            if (Month == 13)
            {
                Month = 1;
                Year++;
            }
            
            ServiceLocator.Instance.HouseholdAggregator.StartNewPeriod(Month, Year);
            ServiceLocator.Instance.UiUpdateManager.newPeriodStartedEvent.Invoke(Month, Year);
        }

        public void CommitCompanyDecision(int companyId, CompanyDecisionStatus status)
        {
            DecisionStati[companyId] = status;
        }

        public bool ProceedWithAutonomous()
        {
            bool areAllDecisionsMade = DecisionStati.All(x => x.Value == CompanyDecisionStatus.Commited);
            if (areAllDecisionsMade && IsGovernmentDecisionCommitted)
            {
                List<int> keys = new List<int>(DecisionStati.Keys);

                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    DecisionStati[keys[i]] = CompanyDecisionStatus.Pending;
                }
                return true;
            }

            return false;
        }
    }
}