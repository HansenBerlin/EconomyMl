using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
using MathNet.Numerics.LinearAlgebra.Single.Solvers;
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
    public class Company : Agent, ICompany
    {
        //public GameObject canvasInfo;
        public GameObject stageZeroBuilding;
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        public PlayerDecisionEvent DecisionRequestEventProp { get; }

        public int OpenPositions { get; private set; } = 0;
        public int ProductStock { get; set; } = 0;
        public decimal ProductPrice { get; private set; } = 1M;
        public decimal OfferedWageRate { get; private set; } = 100;
        public decimal Liquidity { get; set; }
        public int Reputation { get; private set; }
        public int LifetimeMonths { get; private set; } = 1;
        public int Id => GetInstanceID();
        private int _salesInMonth = 0;
        private int _salesLastMonth = 0;
        private readonly List<JobContract> _jobContracts = new();
        private readonly System.Random _rand = new();
        //private bool _initDone;
        private bool _isTraining;
        private bool _writeToDatabase;
        private int _currentActiveIndex = -1;
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
                    $"{Liquidity:0.##}", $"{_salesLastMonth}",
                    _jobContracts.Count.ToString(), ProductStock.ToString(),
                    $"{OfferedWageRate:0}", $"{(_jobContracts.Count > 0 ? _jobContracts.Select(x => x.Wage).Average() : 100):0}", 
                    $"{ProductPrice:0.##}", $"{LifetimeMonths}"
                }, Id);
            }
        }

        private void SetupAgent()
        {
            //ProductStock = 100;
            //OfferedWageRate = ServiceLocator.Instance.LaborMarket.AveragePayment();
            ProductPrice = (ServiceLocator.Instance.ProductMarket.AveragePrice() + ProductPrice) / 2;
            LifetimeMonths = 0;
            //ProductStock = 1000 / ServiceLocator.Instance.Companys.Count * ServiceLocator.Instance.Settings.OutputMultiplier;
            ProductStock = 0;
            Reputation = 100;
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

        public void RequestMonthlyDecision()
        {
            RequestDecision();
        }

        public void StartNextPeriod(decimal price, int workerChange, decimal wage)
        {
            //throw new NotImplementedException();
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
            sensor.AddObservation(_salesLastMonth);
            sensor.AddObservation(_salesInMonth);
            sensor.AddObservation((float)OfferedWageRate);
            sensor.AddObservation((float)ServiceLocator.Instance.LaborMarket.AveragePayment());
            sensor.AddObservation((float)ServiceLocator.Instance.ProductMarket.AveragePrice());
            sensor.AddObservation((float)ServiceLocator.Instance.ProductMarket.DemandForProduct);
            sensor.AddObservation((float)ServiceLocator.Instance.LaborMarket.DemandForWorkforce);
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (_jobContracts.Count == 0)
            {
                actionMask.SetActionEnabled(0, 0, false);
                actionMask.SetActionEnabled(0, 2, false);
            }
        }

        private int _lastMonth;
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            //UpdateCanvasText(false);
            if (_lastMonth == ServiceLocator.Instance.FlowController.Month)
            {
                return;
            }
            
            _lastMonth = ServiceLocator.Instance.FlowController.Month;

            int workerDecision = actionBuffers.DiscreteActions[0];
            float newWage = MapValue(actionBuffers.ContinuousActions[0], 20, 200);
            float newPrice = MapValue(actionBuffers.ContinuousActions[1], 0.1F, 2F);
            
            ProductPrice = (decimal)newPrice;
            OfferedWageRate = (decimal)newWage;
            
            foreach (var contract in _jobContracts)
            {
                if (contract.Wage < OfferedWageRate)
                {
                    contract.Wage = OfferedWageRate;
                }
            }

            if (workerDecision == 1)
            {
                OpenPositions = (int)MapValue(actionBuffers.ContinuousActions[2], 1, 50);
                for (int i = 0; i < OpenPositions; i++)
                {
                    var jobBid = new JobBid(this, (decimal)newWage);
                    ServiceLocator.Instance.LaborMarket.AddJobBid(jobBid);
                    Academy.Instance.StatsRecorder.Add("Market/Job-Bid-Price", newWage);
                }
            }

            if (workerDecision == 2 && _jobContracts.Count > 0)
            {
                int fireWorkers = (int)MapValue(actionBuffers.ContinuousActions[3], 1, _jobContracts.Count);
                for (var i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    if (fireWorkers == 0)
                    {
                        break;
                    }
                    var contract = _jobContracts[i];
                    if (contract.RunsFor > 3)
                    {
                        contract.QuitContract(true);
                        fireWorkers--;
                        Academy.Instance.StatsRecorder.Add("Labor/Fire", 1);

                    }
                }
            }


            SetBuilding();
            //UpdateCanvasText(false);
            ServiceLocator.Instance.FlowController.CommitDecision();
            UpdateCanvasText(false);

        }

        private static float MapValue(float value, float minValue, float maxValue)
        {
            float mappedValue = (value + 1f) * 0.5f * (maxValue - minValue) + minValue;
            return mappedValue;
        }

        public void Produce()
        {
            ProductStock += _jobContracts.Count * ServiceLocator.Instance.Settings.OutputMultiplier;
            if (ProductStock > 0)
            {
                var offer = new ProductOffer(ProductType.Food, this, ProductPrice, ProductStock);
                ServiceLocator.Instance.ProductMarket.AddOffer(offer);
                Academy.Instance.StatsRecorder.Add("Market/P-OfferCount", ProductStock);
                Academy.Instance.StatsRecorder.Add("Market/P-OfferPrice", (float)ProductPrice);
            }
            UpdateCanvasText(false);
        }

        private int lastWorkers;
        
        public void EndMonth()
        {
            if (_writeToDatabase)
            {
                var ledger = new CompanyLedger
                {
                    companyId = Id,
                    openPositions = OpenPositions,
                    month = ServiceLocator.Instance.FlowController.Month,
                    year = ServiceLocator.Instance.FlowController.Year,
                    liquidity = (double)Liquidity,
                    realWage = _jobContracts.Count > 0 ? (double)_jobContracts.Average(x => x.Wage) : 0,
                    workers = _jobContracts.Count,
                    price = (double)ProductPrice,
                    wage = (double)OfferedWageRate,
                    sales = _salesInMonth,
                    stock = ProductStock,
                    lifetime = LifetimeMonths,
                    sessionId = ServiceLocator.Instance.SessionId,
                    emergencyRounds = 0,
                    isStartup = false,
                    isTraining = _isTraining
                };
                StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
            }

            

            for (var i = _jobContracts.Count - 1; i >= 0; i--)
            {
                var contract = _jobContracts[i];
                if (contract.Wage < Liquidity)
                {
                    contract.PayWorker();
                }
                else
                {
                    contract.ReduceWage();
                    Reputation--;
                    AddReward(-0.1F);
                }
            }
            
            if(Liquidity < 10 || _jobContracts.Count == 0)
            {
                Reputation--;
                AddReward(-0.1F);
            }

            if (_jobContracts.Count > lastWorkers)
            {
                AddReward(0.1F);
            }
            else if (_jobContracts.Count == lastWorkers)
            {
                AddReward(0.01F);
            }
            else
            {
                AddReward(-0.11F);
            }

            //PaySocialFare();
            
            LifetimeMonths++;
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            Academy.Instance.StatsRecorder.Add("Company/LastSales", _salesLastMonth);
            Academy.Instance.StatsRecorder.Add("Company/CurrentSales", _salesInMonth);
            //Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)Liquidity);
            Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
            Academy.Instance.StatsRecorder.Add("Company/Rep", Reputation);
            Academy.Instance.StatsRecorder.Add("Product/Price", (float)ProductPrice);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _jobContracts.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            _salesLastMonth = _salesInMonth;
            _salesInMonth = 0;
            
            //UpdateCanvasText(false);
            AddReward(Reputation*0.01F);
            AddReward((float)Liquidity*0.01F);
            //AddReward(_jobContracts.Count*1F);
            if (Reputation >= 1000)
            {
                AddReward(1000F);
                //ProductStock = 0;
                EndEpisode();
            }
            else if (Reputation <= -1000)
            {
                for (int i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    //var contract = _jobContracts[i];
                    //contract.QuitContract();
                }
                SetReward(-100F);
                //ProductStock = 0;
                EndEpisode();
            }
            
            SetBuilding();
            UpdateCanvasText(false);


            
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
            lastWorkers = _jobContracts.Count;
        }


        private void PaySocialFare()
        {
            decimal baseSafety = 100000 / (decimal) ServiceLocator.Instance.Companys.Count / 4;
            var liquidity = Liquidity;
            if (Liquidity > baseSafety)
            {
                //AddReward((float)Liquidity - (float)baseSafety);
                decimal companyReserve = (Liquidity - baseSafety) * 0.10M;
                var unemployed =
                    ServiceLocator.Instance.LaborMarket.Workers.Where(
                        x => x.HasJob == false && x.Money < 100).ToList();

                if (unemployed.Count > 0)
                {
                    decimal societyShare = (Liquidity - baseSafety - companyReserve) / unemployed.Count;
                    foreach (var worker in unemployed)
                    {
                        worker.Give(societyShare);
                    }
                    Liquidity = companyReserve + baseSafety;
                    Academy.Instance.StatsRecorder.Add("Labor/Social", (float)societyShare * unemployed.Count);
                }


                var companys = ServiceLocator.Instance.Companys.Select(x => x.Liquidity).Sum();
                var workers = ServiceLocator.Instance.LaborMarket.Workers.Select(x => x.Money).Sum();
                if (companys + workers < 99999.9M || companys + workers > 100000.1M)
                {
                    Debug.LogError("Society is bankrupt " + Liquidity + " " + liquidity);
                }

            }
            //AddReward((float)societyShare);

            //var unemployed =
            //    ServiceLocator.Instance.LaborMarket.Workers.Where(
            //        x => x.HasJob == false && x.Money < 100).ToList();
            //double societyShare = unemployed.Count > 0 ? (Liquidity - companyReserve) / unemployed.Count : 0;
//
            //foreach (var worker in unemployed)
            //{
            //    worker.Pay(societyShare);
            //}
//
            //Liquidity = societyShare > 0 ? companyReserve : Liquidity;
        }

        public void AddContract(JobContract contract)
        {
            _jobContracts.Add(contract);
            Reputation++;
            AddReward(1F);
        }
        
        public void RemoveContract(JobContract contract, bool isQuitByEmployer)
        {
            _jobContracts.Remove(contract);
            Reputation--;
            AddReward(-1.1F);
        }
        
        public void FullfillBid(ProductType product, int count, decimal price)
        {
            ProductStock -= count;
            Liquidity += count * price;
            //Reputation++;
            _salesInMonth += count;
            AddReward((float)count / 10000);
        }
    }
}
