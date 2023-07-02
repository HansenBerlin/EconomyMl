using System.Collections.Generic;
using NewScripts.Enums;
using UnityEngine.Events;

namespace NewScripts
{
    [System.Serializable]
    public class CompanySelectedEvent : UnityEvent<ICompany> { }
    
    [System.Serializable]
    public class ProductMarketUpdateEvent : UnityEvent<PriceAnalysisStatsModel> { }
    
    [System.Serializable]
    public class CompanyPanelSelectionEvent : UnityEvent<CompanyPanelSelection> { }
    
    [System.Serializable]
    public class PeriodIncrementEvent : UnityEvent<int, int> { }
    
    [System.Serializable]
    public class PeriodAggregateAddedEvent : UnityEvent<Aggregate> { }
}