using UnityEngine;
using UnityEngine.Serialization;

namespace Policies
{
    public class FederalServicesPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("ServiceUnitsPerPersonInPopulation")]
        public int serviceUnitsPerPersonInPopulation;

        [FormerlySerializedAs("IncomeTaxRate")]
        public float incomeTaxRate;

        [FormerlySerializedAs("ProfitTaxRate")]
        public float profitTaxRate;

        [FormerlySerializedAs("ConsumerTaxRate")]
        public float consumerTaxRate;

        [FormerlySerializedAs("FederalWorkerSalary")]
        public float federalWorkerSalary;
    }
}