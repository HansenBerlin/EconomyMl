using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using NewScripts.Ui;
using NewScripts.Ui.Company;
using Unity.VisualScripting;
using UnityEngine;

namespace NewScripts
{
    public class ServiceLocator : MonoBehaviour
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public static ServiceLocator Instance { get; private set; }
        public ProductMarket ProductMarket { get; private set; }
        public LaborMarket LaborMarket { get; private set; }
        public List<ICompany> Companys { get; set; } = new();
        public FlowController FlowController { get; } = new();
        public SetupSettings Settings { get; } = new();
        public StatsSink Stats { get; private set; }
        public PanelActivator CompanyPanel { get; set; }
        public HouseholdAggregatorService HouseholdAggregator { get; set; }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            ProductMarket = GetComponentInChildren<ProductMarket>();
            LaborMarket = GetComponentInChildren<LaborMarket>();
            Stats = GetComponentInChildren<StatsSink>();
            HouseholdAggregator = GetComponentInChildren<HouseholdAggregatorService>();
        }
        
        
    }
}