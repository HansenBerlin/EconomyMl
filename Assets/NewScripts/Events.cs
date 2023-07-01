using System.Collections.Generic;
using NewScripts.Enums;
using UnityEngine.Events;

namespace NewScripts
{
    [System.Serializable]
    // Decisions (worker count change, wage, price) and company id
    public class PlayerDecisionEvent : UnityEvent<int, decimal, decimal, int> { }
    
    [System.Serializable]
    public class CompanySelectedEvent : UnityEvent<ICompany> { }
    
    [System.Serializable]
    public class ProductMarketUpdateEvent : UnityEvent<List<ProductOffer>, List<ProductBid>, List<Deal>> { }
    
    [System.Serializable]
    public class CompanyPanelSelectionEvent : UnityEvent<CompanyPanelSelection> { }
}