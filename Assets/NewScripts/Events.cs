using System.Collections.Generic;
using NewScripts.Enums;
using UnityEngine.Events;

namespace NewScripts
{
    [System.Serializable]
    public class PlayerDecisionEvent : UnityEvent<int, decimal, decimal> { }
    
    [System.Serializable]
    public class ProductMarketUpdateEvent : UnityEvent<List<ProductOffer>, List<ProductBid>, List<Deal>> { }
    
    [System.Serializable]
    public class CompanyPanelSelectionEvent : UnityEvent<CompanyPanelSelection> { }
}