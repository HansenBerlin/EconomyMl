using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
using NewScripts.Http;
using NewScripts.Http;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts
{
    public class Company : Agent
    {
        //public GameObject canvasInfo;
        public GameObject stageZeroBuilding;
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        public int OpenPositions { get; private set; } = 0;
        public int ProductStock { get; set; } = 0;
        public double ProductPrice { get; private set; } = 0.5;
        //public double OfferedWageRate { get; private set; } = 100;
        public double Liquidity { get; set; }
        public int Reputation { get; private set; }
        public int LifetimeMonths { get; private set; } = 1;
        private int Id => GetInstanceID();
        private int _salesInMonth = 0;
        private int _salesLastMonth = 0;
        private readonly List<JobContract> _jobContracts = new();
        private readonly System.Random _rand = new();
        //private bool _initDone;
        private bool _isTraining;
        private bool _writeToDatabase;
        private int _currentActiveIndex;
        //private int _startUpRounds = 12;
        
        private void Start()
        {
            _isTraining = ServiceLocator.Instance.Settings.IsTraining;
            _writeToDatabase = ServiceLocator.Instance.Settings.WriteToDatabase;
            //SetupAgent();
        }

        private void Update() {  
            if (Input.GetMouseButtonDown(0)) {  
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {  
                    if (hit.transform == transform) {  
                        UpdateCanvasText(true);
                    }  
                }  
            }  
        }
        
        private void UpdateCanvasText(bool isClick)
        {
            if (ServiceLocator.Instance.PopupInfoService.CurrentlyActive == Id || isClick)
            {
                ServiceLocator.Instance.PopupInfoService.SetTexts(new List<string>
                {
                    $"{Liquidity:0.##}", _salesInMonth.ToString(),
                    _jobContracts.Count.ToString(), ProductStock.ToString(),
                    $"{OfferedWageRate:0}", $"{RealwageRate:0}", 
                    $"{ProductPrice:0.##}", $"LT:{LifetimeMonths} E:{_emergencyRounds} SU:{_startUpRounds}"
                }, Id);
            }
        }

        private void SetupAgent()
        {
            //ProductStock = 100;
            //OfferedWageRate = ServiceLocator.Instance.LaborMarket.AveragePayment();
            ProductPrice = ServiceLocator.Instance.ProductMarket.AveragePrice();
            SetBuilding();
        }

        private void SetBuilding()
        {
            var buildings = new List<GameObject>
            {
                stageOneBuilding, stageTwoBuilding, stageThreeBuilding
            };
            double companycount = ServiceLocator.Instance.Companys.Count;
            double personCount = ServiceLocator.Instance.LaborMarket.Workers.Count;
            double ratio = personCount / companycount;
            int activeIndex = _jobContracts.Count < ratio * 2 ? 0 : _jobContracts.Count > ratio * 5 ? 2 : 1;
            if (activeIndex != _currentActiveIndex)
            {
                for (var i = 0; i < buildings.Count; i++)
                {
                    buildings[i].SetActive(i == activeIndex);
                }

                _currentActiveIndex = activeIndex;
            }
        }

        public override void OnEpisodeBegin()
        {
            SetupAgent();
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation((float)Liquidity);
            sensor.AddObservation(_jobContracts.Count);
            sensor.AddObservation((float)ProductPrice);
            sensor.AddObservation(ProductStock);
            sensor.AddObservation(OpenPositions);
            sensor.AddObservation((float)OfferedWageRate);
            sensor.AddObservation(_salesLastMonth);
            sensor.AddObservation(_salesInMonth);
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (ServiceLocator.Instance.FlowController.Day != 1 || _decisionMade)
            {
                _decisionMade = ServiceLocator.Instance.FlowController.Day == 1;
                return;
            }

            float newWage = (actionBuffers.ContinuousActions[0] + 1) / 2 * 100 + 1;
            float newPrice = (actionBuffers.ContinuousActions[1] + 1 ) / 2 * 10 + 0.01F;
            int fireWorkers = actionBuffers.DiscreteActions[0];
            int openPositions = actionBuffers.DiscreteActions[1];

            OfferedWageRate = newWage;
            ProductPrice = newPrice;
            OpenPositions = openPositions;
            
            for (var i = _jobContracts.Count - 1; i >= 0; i--)
            {
                if (fireWorkers == 0)
                {
                    break;
                }
                var worker = _jobContracts[i];
                _jobContracts.Remove(worker);
                worker.Fire();
                fireWorkers--;
            }

            _decisionMade = true;
            SetBuilding();
            UpdateCanvasText(false);
        }

        public void Produce()
        {
            ProductStock += _jobContracts.Count * ServiceLocator.Instance.Settings.OutputMultiplier;
            UpdateCanvasText(false);
        }
        
        public void EndMonth()
        {
            //if (_writeToDatabase)
            //{
            //    var ledger = new CompanyLedger
            //    {
            //        companyId = Id,
            //        openPositions = OpenPositions,
            //        month = ServiceLocator.Instance.FlowController.Month,
            //        year = ServiceLocator.Instance.FlowController.Year,
            //        liquidity = Liquidity,
            //        realWage = RealwageRate,
            //        workers = _workers.Count,
            //        price = ProductPrice,
            //        wage = OfferedWageRate,
            //        sales = _salesInMonth,
            //        stock = ProductStock,
            //        lifetime = LifetimeMonths,
            //        sessionId = ServiceLocator.Instance.SessionId,
            //        emergencyRounds = _emergencyRounds,
            //        isStartup = _startUpRounds > 0 && _startUpRounds < 13,
            //        isTraining = _isTraining
            //    };
            //    StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
            //}

            for (var i = _jobContracts.Count - 1; i >= 0; i--)
            {
                var worker = _jobContracts[i];
                if (worker.Wage < Liquidity)
                {
                    Liquidity -= worker.Pay();
                }
                else
                {
                    worker.Fire();
                    _jobContracts.Remove(worker);
                    Reputation--;
                }
            }

            PaySocialFare();
            
            LifetimeMonths++;
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            _salesLastMonth = _salesInMonth;
            _salesInMonth = 0;
            Academy.Instance.StatsRecorder.Add("Company/LastSales", _salesLastMonth);
            Academy.Instance.StatsRecorder.Add("Company/CurrentSales", _salesInMonth);
            //Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)Liquidity);
            
            Academy.Instance.StatsRecorder.Add("Product/Price", (float)ProductPrice);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
            
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _jobContracts.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            Academy.Instance.StatsRecorder.Add("Labor/Wage", (float)OfferedWageRate);
            Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
            Academy.Instance.StatsRecorder.Add("Company/Reward", (float)CumulatedReward);
            
            UpdateCanvasText(false);

            if (Reputation >= 100)
            {
                SetReward(1F);
                EndEpisode();
            }
            else if (Reputation <= -100)
            {
                for (int i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    var worker = _jobContracts[i];
                    worker.Fire();
                    _jobContracts.Remove(worker);
                }

                EndEpisode();
            }
            
            SetBuilding();

            
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
        }

        private void PaySocialFare()
        {
            double companyReserve = Liquidity * 0.1;
            //AddReward((float)societyShare);

            var unemployed =
                ServiceLocator.Instance.LaborMarket.Workers.Where(
                    x => x.HasJob == false && x.Money < 100).ToList();
            double societyShare = unemployed.Count > 0 ? (Liquidity - companyReserve) / unemployed.Count : 0;

            foreach (var worker in unemployed)
            {
                worker.Pay(societyShare);
            }

            Liquidity = societyShare > 0 ? companyReserve : Liquidity;
        }

        public void AddContract(JobContract contract)
        {
            _jobContracts.Add(contract);
        }
        
        public void QuitJob(JobContract contract)
        {
            _jobContracts.Remove(contract);
            //OpenPositions++;
        }

        private bool _decisionMade;
        
        
        
        
    }
}
