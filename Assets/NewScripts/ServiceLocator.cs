using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public ProductMarket FoodProductMarket { get; private set; }
        public ProductMarket LuxuryProductMarket { get; private set; }
        public LaborMarket LaborMarket { get; private set; }
        public List<ICompany> Companys { get; set; } = new();
        public FlowController FlowController { get; set; }
        public Settings Settings { get; } = new();
        public CompanyPanelController CompanyPanelController { get; set; }
        public HouseholdAggregatorService HouseholdAggregator { get; private set; }
        public UiUpdateManager UiUpdateManager { get; private set; }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            
            FoodProductMarket = GetComponentInChildren<ProductMarket>();
            LuxuryProductMarket = GetComponentInChildren<ProductMarket>();
            LaborMarket = GetComponentInChildren<LaborMarket>();
            HouseholdAggregator = GetComponentInChildren<HouseholdAggregatorService>();
            UiUpdateManager = GetComponentInChildren<UiUpdateManager>();
            GetComponentInChildren<FooterMenuController>().RegisterEvents();
        }

        public void InitFlowController()
        {
            if (Companys.Count == 0)
            {
                throw new Exception("No companys found");
            }
            FlowController = new FlowController(Companys.Select(c => c.Id).ToList());
        }
    }
}