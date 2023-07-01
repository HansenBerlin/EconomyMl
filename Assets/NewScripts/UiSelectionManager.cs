using NewScripts.Enums;
using UnityEngine;

namespace NewScripts
{
    public class UiSelectionManager : MonoBehaviour
    {
        public CompanySelectedEvent companySelectedEvent;
        public PlayerDecisionEvent playerDecisionEvent;

        private ICompany _selectedCompany;
        
        public void BroadcastUpdateDecisionValuesEvent(int id)
        {
            if (_selectedCompany is {PlayerType: PlayerType.Human} && _selectedCompany.Id == id)
            {
                playerDecisionEvent.Invoke(
                    _selectedCompany.WorkerCount,
                    _selectedCompany.OfferedWageRate,
                    _selectedCompany.ProductPrice,
                    _selectedCompany.Id);
            }
        }

        public void CompanySelectionEvent(ICompany company)
        {
            if (_selectedCompany.Id != company.Id)
            {
                _selectedCompany = company;
                companySelectedEvent.Invoke(company);
                BroadcastUpdateDecisionValuesEvent(company.Id);
            }
        }
    }
}