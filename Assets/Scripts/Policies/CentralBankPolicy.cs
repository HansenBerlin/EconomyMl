using UnityEngine;
using UnityEngine.Serialization;

namespace Policies
{
    public class CentralBankPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("MinimumEquityRate")] public float minimumEquityRate;
        [FormerlySerializedAs("LeaseInterestRate")] public float leaseInterestRate;

    }
}