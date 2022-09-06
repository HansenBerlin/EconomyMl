using UnityEngine;

namespace Policies
{
    public class PoliciesWrapper : MonoBehaviour
    {
        public GameObject ageBoundaryPolicyGo;
        public GameObject educationBoundaryPolicyGo;
        public GameObject workerPolicyGo;
        public GameObject federalServicesPolicyGo;
        public GameObject centralBankPolicyGo;
        public AgeBoundaryPolicy AgeBoundaries { get; private set; }
        public EducationBoundaryPolicy EducationBoundaries { get; private set; }
        public FederalUnemployedPaymentPolicy FederalUnemployedPaymentPolicies { get; private set; }
        public FederalServicesPolicy FederalPolicies { get; private set; }
        public CentralBankPolicy CentralBankPolicies { get; private set; }

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