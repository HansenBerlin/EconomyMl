using Policies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Settings
{



    public class PoliciesWrapper : MonoBehaviour
    {
        public AgeBoundaryPolicy AgeBoundaries { get; private set;}
        public EducationBoundaryPolicy EducationBoundaries { get; private set;}
        public FederalUnemployedPaymentPolicy FederalUnemployedPaymentPolicies { get; private set; }
        public FederalServicesPolicy FederalPolicies { get; private set; }
        public CentralBankPolicy CentralBankPolicies { get; private set; }

        [FormerlySerializedAs("AgeBoundaryPolicyGo")] public GameObject ageBoundaryPolicyGo;
        [FormerlySerializedAs("EducationBoundaryPolicyGo")] public GameObject educationBoundaryPolicyGo;
        [FormerlySerializedAs("WorkerPolicyGo")] public GameObject workerPolicyGo;
        [FormerlySerializedAs("FederalServicesPolicyGo")] public GameObject federalServicesPolicyGo;
        [FormerlySerializedAs("CentralBankPolicyGo")] public GameObject centralBankPolicyGo;

        public void Awake()
        {
            AgeBoundaries = ageBoundaryPolicyGo.GetComponent<AgeBoundaryPolicy>();
            EducationBoundaries = educationBoundaryPolicyGo.GetComponent<EducationBoundaryPolicy>();
            FederalUnemployedPaymentPolicies = workerPolicyGo.GetComponent<FederalUnemployedPaymentPolicy>();
            FederalPolicies = federalServicesPolicyGo.GetComponent<FederalServicesPolicy>();
            CentralBankPolicies = centralBankPolicyGo.GetComponent<CentralBankPolicy>();
        }
    }
}