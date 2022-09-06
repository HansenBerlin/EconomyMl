using System;
using UnityEngine;

namespace Models
{
    public class EnvironmentModel : MonoBehaviour
    {
        [field: SerializeField] public string CountryName;

        [field: SerializeField] public int Month { get; set; } = 1;

        public int Year => (int) Math.Floor((double) Month / 12);
    }
}