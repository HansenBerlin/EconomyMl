using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Interfaces;
using UnityEngine.Events;

namespace NewScripts.Game.Flow
{
    [System.Serializable]
    public class CompanySelectedEvent : UnityEvent<ICompany> { }
    
    [System.Serializable]
    public class MarketUpdateEvent : UnityEvent<PriceAnalysisStatsModel> { }
    
    [System.Serializable]
    public class CompanyPanelSelectionEvent : UnityEvent<CompanyPanelSelection> { }
    
    [System.Serializable]
    public class PeriodIncrementEvent : UnityEvent<int, int> { }
    
    [System.Serializable]
    public class PeriodAggregateAddedEvent : UnityEvent<Aggregate> { }
}