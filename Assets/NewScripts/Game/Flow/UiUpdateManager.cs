using NewScripts.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Game.Flow
{
    public class UiUpdateManager : MonoBehaviour
    {
        public CompanySelectedEvent companySelectedEvent = new();
        public CompanySelectedEvent playerDecisionValuesUpdateEvent = new();
        public PeriodIncrementEvent newPeriodStartedEvent = new();
        [FormerlySerializedAs("productsUpdatedEvent")] public MarketUpdateEvent updatedEvent = new();

        public int SelectedCompanyId { get; private set; }
        
        public void BroadcastUpdateDecisionValuesEvent(ICompany company)
        {
            if (SelectedCompanyId == company.Id)
            {
                playerDecisionValuesUpdateEvent.Invoke(company);
            }
        }

        public void SelectCompanyEvent(ICompany company)
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