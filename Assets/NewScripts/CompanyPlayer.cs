using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
using MathNet.Numerics.LinearAlgebra.Single.Solvers;
using Mirror;
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
    public class CompanyPlayer : MonoBehaviour, ICompany
    {
        //public GameObject canvasInfo;
        public GameObject stageZeroBuilding;
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        public int OpenPositions { get; private set; } = 0;
        public int ProductStock { get; set; } = 0;
        public decimal ProductPrice { get; private set; } = 1M;
        public decimal OfferedWageRate { get; private set; } = 100;
        public decimal Liquidity { get; set; }
        public int Reputation { get; private set; }
        public double Reward { get; private set; }
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
        private PlayerDecisionEvent DecisionRequestEvent;
        public PlayerDecisionEvent DecisionRequestEventProp => DecisionRequestEvent;
        
        private void Awake()
        {
            _isTraining = ServiceLocator.Instance.Settings.IsTraining;
            _writeToDatabase = ServiceLocator.Instance.Settings.WriteToDatabase;
            DecisionRequestEvent ??= new PlayerDecisionEvent();
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


        private void AddReward(double reward)
        {
            Reward += reward;
        }
        
        private int _lastMonth;
        
        public void SendDecision(decimal price, int workerChange, decimal wage)
        {
            if (_lastMonth == ServiceLocator.Instance.FlowController.Month)
            {
                return;
            }
            
            _lastMonth = ServiceLocator.Instance.FlowController.Month;

            ProductPrice = price;
            OfferedWageRate = wage;

            foreach (var contract in _jobContracts)
            {
                if (contract.Wage < OfferedWageRate)
                {
                    contract.Wage = OfferedWageRate;
                }
            }
            
            if (workerChange > 0)
            {
                OpenPositions = workerChange;
                for (int i = 0; i < OpenPositions; i++)
                {
                    var jobBid = new JobBid(this, (decimal)wage);
                    ServiceLocator.Instance.LaborMarket.AddJobBid(jobBid);
                }
            }

            if (workerChange < 0 && _jobContracts.Count > 0)
            {
                int fireWorkers = workerChange * -1;
                for (var i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    if (fireWorkers == 0)
                    {
                        break;
                    }
                    var contract = _jobContracts[i];
                    if (contract.RunsFor > 3)
                    {
                        contract.QuitContract();
                        fireWorkers--;
                    }
                }
            }


            SetBuilding();
            //UpdateCanvasText(false);
            UpdateCanvasText(false);

            ServiceLocator.Instance.FlowController.CommitDecision();
        }

        public void RequestMonthlyDecision()
        {
            DecisionRequestEvent.Invoke(_jobContracts.Count, OfferedWageRate, ProductPrice);
        }

        public void Produce()
        {
            ProductStock += _jobContracts.Count * ServiceLocator.Instance.Settings.OutputMultiplier;
            if (ProductStock > 0)
            {
                var offer = new ProductOffer(ProductType.Food, this, ProductPrice, ProductStock);
                ServiceLocator.Instance.ProductMarket.AddOffer(offer);
            }
            //UpdateCanvasText(false);
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
                //EndEpisode();
            }
            else if (Reputation <= -1000)
            {
                for (int i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    //var contract = _jobContracts[i];
                    //contract.QuitContract();
                }
                AddReward(-100F);
                //ProductStock = 0;
                //EndEpisode();
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
        
        public void RemoveContract(JobContract contract)
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
