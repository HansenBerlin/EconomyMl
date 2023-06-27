using UnityEngine.Events;

namespace NewScripts
{
    [System.Serializable]
    public class PlayerDecisionRequestEvent : UnityEvent<int, decimal, decimal>
    {
        
    }
}