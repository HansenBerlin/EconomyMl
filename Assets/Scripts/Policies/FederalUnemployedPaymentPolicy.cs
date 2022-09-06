using UnityEngine;
using UnityEngine.Serialization;

namespace Policies
{
    public class FederalUnemployedPaymentPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("GuaranteedIncome")]
        public float guaranteedIncome;

        [FormerlySerializedAs("UnemployedSupportRate")]
        public float unemployedSupportRate;

        [FormerlySerializedAs("UnemployedSupportMax")]
        public float unemployedSupportMax;

        [FormerlySerializedAs("UnemployedSupportMin")]
        public float unemployedSupportMin;

        [FormerlySerializedAs("RetirementSupportRate")] [FormerlySerializedAs("RetirementRateSupporRate")]
        public float retirementSupportRate;
    }
}