using System;
using UnityEngine;

namespace Models.Meta
{



    public class EnvironmentModel : MonoBehaviour
    {
        [field:SerializeField]
        public int Month { get; set; }
        
        public int Day { get; set; }
        
        public int Year => (int) Math.Floor((double) Month / 12);
        
        [field:SerializeField]
        public string CountryName { get; private set; }

        /*public EnvironmentModel(string countryName, int month)
        {
            CountryName = countryName;
            Month = month;
        }*/
        
        /*public void Init(string countryName, int month)
        {
            CountryName = countryName;
            Month = month;
        }*/

    }
}