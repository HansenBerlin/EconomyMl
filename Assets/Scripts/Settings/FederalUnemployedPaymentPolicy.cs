using UnityEngine;
using UnityEngine.Serialization;

namespace Settings
{
    public class FederalUnemployedPaymentPolicy : MonoBehaviour
    {
        public float GuaranteedIncome;
        public float UnemployedSupportRate;
        public float UnemployedSupportMax;
        public float UnemployedSupportMin;
        [FormerlySerializedAs("RetirementRateSupporRate")] public float RetirementSupportRate;
    }
}