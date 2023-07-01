using NewScripts.Enums;
using UnityEngine;

namespace NewScripts
{
    public class UiUpdateManager : MonoBehaviour
    {
        public CompanySelectedEvent companySelectedEvent;
        public CompanySelectedEvent playerDecisionValuesUpdateEvent;
        public PeriodIncrementEvent newPeriodStartedEvent;

        public int SelectedCompanyId { get; private set; }
        
        public void BroadcastUpdateDecisionValuesEvent(ICompany company)
        {
            if (company is {PlayerType: PlayerType.Human} && SelectedCompanyId == company.Id)
            {
                playerDecisionValuesUpdateEvent.Invoke(company);
            }
        }

        public void CompanyUpdateValuesEvent(ICompany company)
        {
            if (SelectedCompanyId != company.Id)
            {
                SelectedCompanyId = company.Id;
                companySelectedEvent.Invoke(company);
                BroadcastUpdateDecisionValuesEvent(company);
            }
        }
    }
}