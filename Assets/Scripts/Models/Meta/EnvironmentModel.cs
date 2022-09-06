using System;
using UnityEngine;

namespace Assets.Scripts.Models.Meta
{



    public class EnvironmentModel : MonoBehaviour
    {
        [field: SerializeField] public int Month { get; set; } = 1;
        
        public int Day { get; set; }
        
        public int Year => (int) Math.Floor((double) Month / 12);
        
        [field:SerializeField]
        public string CountryName { get; private set; }

    }
}