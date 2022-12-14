using UnityEngine;
using UnityEngine.Serialization;

namespace Policies
{
    public class AgeBoundaryPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("AdultMinAge")] public int adultMinAge;
        [FormerlySerializedAs("WorkerMaxAge")] public int workerMaxAge;
    }
}