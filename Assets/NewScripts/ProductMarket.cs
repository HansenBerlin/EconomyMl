using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace NewScripts
{
    [System.Serializable]
    public class CompanyPayEvent : UnityEvent<string>
    {
    }
    
    public class ProductMarket : MonoBehaviour
    {
        //private LaborMarket _laborMarket;
        //public CompanyPayEvent payCompanyEvent;

        private readonly Random _rand = new();

        public void Awake()
        {
            //if (payCompanyEvent == null)
            //{
            //    payCompanyEvent = new CompanyPayEvent();
            //}
        }

        
    }
}