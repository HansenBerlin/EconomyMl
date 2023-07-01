using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
using UnityEngine;

namespace NewScripts
{
    public class FlowController
    {
        public int Year { get; private set; } = 1;
        public int Month { get; private set; } = 1;
        private Dictionary<int, CompanyDecisionStatus> DecisionStati { get; set; } = new();

        public FlowController(List<int> companyIds)
        {
            foreach (var companyId in companyIds)
            {
                DecisionStati.Add(companyId, CompanyDecisionStatus.Requested);
            }
        }

        public void IncrementMonth()
        {
            if (DecisionStati.All(x => x.Value == CompanyDecisionStatus.Pending) == false)
            {
                throw new Exception("Not all companys have commited");
            }
            
            Month++;
            if (Month == 13)
            {
                Month = 1;
                Year++;
            }
            foreach (var item in DecisionStati)
            {
                DecisionStati[item.Key] = CompanyDecisionStatus.Requested;
            }
            ServiceLocator.Instance.HouseholdAggregator.StartNewPeriod(Month, Year);
        }

        public void CommitDecision(int companyId)
        {
            DecisionStati[companyId] = CompanyDecisionStatus.Commited;
        }
        
        public bool Proceed()
        {
            if (DecisionStati.All(x => x.Value == CompanyDecisionStatus.Commited))
            {
                foreach (var item in DecisionStati)
                {
                    DecisionStati[item.Key] = CompanyDecisionStatus.Pending;
                }
                return true;
            }

            return false;
        }
    }
}