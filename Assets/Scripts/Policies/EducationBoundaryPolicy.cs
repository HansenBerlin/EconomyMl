using UnityEngine;
using UnityEngine.Serialization;

namespace Policies
{
    public class EducationBoundaryPolicy : MonoBehaviour
    {
        [FormerlySerializedAs("AgeToStartSchool")]
        public int ageToStartSchool;

        [FormerlySerializedAs("MinYearsInSchool")]
        public int minYearsInSchool;

        [FormerlySerializedAs("MaxYearsInSchool")]
        public int maxYearsInSchool;
    }
}