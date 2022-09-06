using UnityEngine;
using UnityEngine.Serialization;

namespace Settings
{

    public class AgeBoundaryPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("AdultMinAge")] public int adultMinAge;
        [FormerlySerializedAs("WorkerMaxAge")] public int workerMaxAge;
    }
}