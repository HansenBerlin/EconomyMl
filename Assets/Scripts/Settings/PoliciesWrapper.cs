using UnityEngine;

namespace Settings
{



    public class PoliciesWrapper : MonoBehaviour
    {
        public AgeBoundaryPolicy AgeBoundaries { get; private set;}
        public EducationBoundaryPolicy EducationBoundaries { get; private set;}
        public FederalUnemployedPaymentPolicy federalUnemployedPaymentPolicies { get; private set; }
        public FederalServicesPolicy FederalPolicies { get; private set; }

        public GameObject AgeBoundaryPolicyGo;
        public GameObject EducationBoundaryPolicyGo;
        public GameObject WorkerPolicyGo;
        public GameObject FederalServicesPolicyGo;

        public void Awake()
        {
            AgeBoundaries = AgeBoundaryPolicyGo.GetComponent<AgeBoundaryPolicy>();
            EducationBoundaries = EducationBoundaryPolicyGo.GetComponent<EducationBoundaryPolicy>();
            federalUnemployedPaymentPolicies = WorkerPolicyGo.GetComponent<FederalUnemployedPaymentPolicy>();
            FederalPolicies = FederalServicesPolicyGo.GetComponent<FederalServicesPolicy>();
        }
    }
}