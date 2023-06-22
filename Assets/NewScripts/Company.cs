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
        public GameObject startupSign;
        private BoxCollider _boxCollider;
        private int currentActiveIndex = 0;
        public bool IsBlocked => _startUpRounds is > 13;
        public int id = 0;
        public int OpenPositions { get; private set; } = 0;
        private int ProductionInMonth = 0;
        private int SalesInMonth = 0;
        private int SalesLastMonth = 0;
        private const int Days = 20;
        public double CumulatedReward { get; private set; }
        public int ProductStock { get; private set; } = 0;

        public double ProductPrice { get; private set; } = 0.5;

        public double WageRate { get; private set; } = 100;
        public double RealwageRate { get; private set; } = 100;

        private readonly List<Worker> _workers = new();

        public int WorkersCount => _workers.Count;

        
        private readonly System.Random _rand = new();
        public int LifetimeMonths { get; private set; } = 1;

        private int Id => GetInstanceID();

        public double Liquidity { get; private set; }
        //public double ProfitInMonth { get; private set; }
        //private int _initialWorkers;

        private bool _initDone;
        private bool _isTraining;
        private bool _writeToDatabase;
        private int _emergencyRounds;
        private int _startUpRounds = 12;

        //private double _initialMoney;

        public void Init(int companiesPerType, bool isTraining, bool writeToDatabase)
        {
            _isTraining = isTraining;
            _writeToDatabase = writeToDatabase;
            id = Id;
            var initialWorkers = (int)Math.Floor(1000 / (double)companiesPerType);
            //_initialMoney = 79000 / (double)companiesPerType;
            //Liquidity = _initialMoney;
            var availableWorkers = ServiceLocator.Instance.LaborMarketService.Workers.Where(x => x.HasJob == false).ToList();
            for (int i = 0; i < initialWorkers; i++)
            {
                availableWorkers[i].InitialJobSetup(this);
                _workers.Add(availableWorkers[i]);
            }
            SetupAgent();
            LifetimeMonths = 1;
            emergencySign.SetActive(false);
            _initDone = true;
            _boxCollider = GetComponent<BoxCollider>();
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
                    $"{Liquidity:0.##}", SalesInMonth.ToString(),
                    _workers.Count.ToString(), ProductStock.ToString(),
                    $"{WageRate:0}", $"{RealwageRate:0}", 
                    $"{ProductPrice:0.##}", $"LT:{LifetimeMonths} E:{_emergencyRounds} SU:{_startUpRounds}"
                }, Id);
            }
        }

        private void SetupAgent()
        {
            //ProductStock = 100;
            WageRate = 100 + (double)_rand.Next(-50, 51);
            RealwageRate = WageRate;
            ProductPrice = 0.5 + (double) _rand.Next(-10, 11) / 100;
            stageTwoBuilding.SetActive(false);
            stageThreeBuilding.SetActive(false);
            OpenPositions = 2;
            _emergencyRounds = 0;
            //StartUpRounds = 24;
            CumulatedReward = 0;
            //SetBuilding();
        }

        private void SetBuilding()
        {
            if (_startUpRounds > 13)
            {
                stageZeroBuilding.SetActive(true);
                stageOneBuilding.SetActive(false);
                stageTwoBuilding.SetActive(false);
                stageThreeBuilding.SetActive(false);
                startupSign.SetActive(false);
                return;
            }
            if (_startUpRounds is < 13 and > 0)
            {
                stageZeroBuilding.SetActive(false);
                stageOneBuilding.SetActive(true);
                startupSign.SetActive(true);
                //return;
            }
            var buildings = new List<GameObject>
            {
                stageOneBuilding, stageTwoBuilding, stageThreeBuilding
            };
            double companycount = ServiceLocator.Instance.Companys.Count;
            double personCount = ServiceLocator.Instance.LaborMarketService.Workers.Count;
            double ratio = personCount / companycount;
            int activeIndex = _workers.Count < ratio * 2 ? 0 : _workers.Count > ratio * 5 ? 2 : 1;
            if (activeIndex != currentActiveIndex)
            {
                for (var i = 0; i < buildings.Count; i++)
                {
                    buildings[i].SetActive(i == activeIndex);
                }

                currentActiveIndex = activeIndex;
            }

            emergencySign.SetActive(_emergencyRounds > 0);
        }

        public override void OnEpisodeBegin()
        {
            SetupAgent();
            if (_initDone)
            {
                //ProductStock = 50;
                //WageRate = 21 + (double) _rand.Next(-100, 101) / 100;
                //ProductPrice = 1 + (double) _rand.Next(-10, 11) / 100;
                
                //emergencySign.SetActive(false);
                
            }
        }

        public Receipt BuyFromCompany(int amount)
        {
            amount = amount > ProductStock ? ProductStock : amount;
            ProductStock -= amount;
            SalesInMonth += amount;
            
            if (ProductPrice <= 0)
            {
                throw new Exception("Less than zero.");
            }
            Liquidity += ProductPrice * amount;
            //ProfitInMonth += ProductPrice * amount;
            LiquidityCheck();
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Liquidity:0}";
            
            //
            float reward = amount * (float) ProductPrice * 1F;
            CumulatedReward += reward;
            AddReward(reward);
            //RequestDecision();

            return new Receipt
            {
                AmountPaid = amount * ProductPrice,
                CountBought = amount
            };
        }

        public void InvestInCompany(double amount)
        {
            Liquidity += amount;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation((float)Liquidity);
            sensor.AddObservation(_workers.Count);
            sensor.AddObservation((float)ProductPrice);
            sensor.AddObservation(ProductStock);
            sensor.AddObservation(OpenPositions);
            //sensor.AddObservation((float)ProfitInMonth);
            sensor.AddObservation((float)WageRate);
            sensor.AddObservation(SalesLastMonth);
            sensor.AddObservation(SalesInMonth);
            sensor.AddObservation(_emergencyRounds);
            sensor.AddObservation((float)RealwageRate);
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (_workers.Count < 2)
            {
                //actionMask.SetActionEnabled(2, 0, false);
            }

            if (_emergencyRounds > 0)
            {
                //actionMask.SetActionEnabled(1, 2, false);
            }

            
            return;
            //if (CurrentPhase != ActionPhase.BeginMonth)
            //{
            //    for (int i = 0; i < 20; i++)
            //    {
            //        bool isEnabled = i == 9; 
            //        actionMask.SetActionEnabled(0, i, isEnabled);
            //        actionMask.SetActionEnabled(1, i, isEnabled);
            //        actionMask.SetActionEnabled(2, i, isEnabled);
            //    }
            //}
        }

        //public void StartMonth()
        //{
        //    Academy.Instance.StatsRecorder.Add("Company/LastSales", SalesLastMonth);
        //    Academy.Instance.StatsRecorder.Add("Company/CurrentSales", SalesInMonth);
        //    Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)Liquidity);
        //    
        //    Academy.Instance.StatsRecorder.Add("Product/Price", (float)ProductPrice);
        //    Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
        //    
        //    Academy.Instance.StatsRecorder.Add("Labor/Workers", _workers.Count);
        //    Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
        //    Academy.Instance.StatsRecorder.Add("Labor/Wage", (float)WageRate);
//
        //    SalesLastMonth = SalesInMonth;
        //    SalesInMonth = 0;
        //    //ProfitInMonth = 0;
        //    RequestDecision();
        //}

        public void StartDay()
        {
            ServiceLocator.Instance.stepsCompany++;
            if (ProductStock < _workers.Count * 60 * 10)
            {
                ProductStock += _workers.Count * 10;
                AddReward(_workers.Count);
                //Debug.LogError("Produced " + _workers.Count * 10 + " Stock: " + ProductStock);
            }
            else
            {
                AddReward(-1);
                //Debug.LogWarning("No production. Stock: " + ProductStock);
            }
            //RequestDecision();
            //ServiceLocator.Instance.FlowController.CommitDecision();
            UpdateCanvasText(false);

        }

        private void LiquidityCheck()
        {
            if (Liquidity is < 0 and > -0.0001)
            {
                Debug.LogWarning("Liquidity below Zero");
                Liquidity = 0;
            }
            else if (Liquidity < -0.0001)
            {
                //Debug.LogError("Liquidity below Zero");
                throw new Exception("Liquidity below Zero: " + Liquidity);
            }
        }

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
                    liquidity = Liquidity,
                    realWage = RealwageRate,
                    workers = _workers.Count,
                    price = ProductPrice,
                    wage = WageRate,
                    sales = SalesInMonth,
                    stock = ProductStock,
                    lifetime = LifetimeMonths,
                    sessionId = ServiceLocator.Instance.SessionId,
                    emergencyRounds = _emergencyRounds,
                    isStartup = _startUpRounds > 0 && _startUpRounds < 13,
                    isTraining = _isTraining
                };
                StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
            }

            LiquidityCheck();

            if (_startUpRounds > 12)
            {
                _startUpRounds--;
                UpdateCanvasText(false);
                return;
            }
            
            double estimatedWorkerPayments = WageRate * _workers.Count;
            //Liquidity += ProfitInMonth;
            //ProfitInMonth = 0;

            if (estimatedWorkerPayments > Liquidity && _workers.Count > 0)
            {
                RealwageRate = Liquidity / _workers.Count * 0.98;
                foreach (var worker in _workers)
                {
                    worker.Pay(RealwageRate);
                    Liquidity -= RealwageRate;
                    //AddReward((float)(WageRate / 100));
                }
                //LiquidityCheck();
                //AddReward((float)(Liquidity - estimatedWorkerPayments));
                //EmergencyRounds++;
                _emergencyRounds = _startUpRounds > 0 ? 0 : RealwageRate == 0 ? 12 : _emergencyRounds + 1;
                AddReward(-1);
            }
            else if (_workers.Count == 0 && SalesInMonth == 0 || Liquidity < 10)
            {
                if (_startUpRounds == 0)
                {
                    AddReward(-1);
                    _emergencyRounds++;
                }
                //EmergencyRounds = StartUpRounds == 0 ? EmergencyRounds + 1 : 0;
            }
            else if (_workers.Count > 0 && estimatedWorkerPayments < Liquidity)
            {
                RealwageRate = WageRate;
                foreach (var worker in _workers)
                {
                    worker.Pay(WageRate);
                    Liquidity -= WageRate;
                    //AddReward((float)(WageRate / 100));
                }
                LiquidityCheck();
            
                if (Liquidity > 0)
                {
                    CumulatedReward += (float) Liquidity;
                    AddReward((float)Liquidity * 10);
                    PaySocialFare();
                }
                else
                {
                    AddReward(1);
                }

                _emergencyRounds = 0;
            }
            else if (Liquidity > 0)
            {
                PaySocialFare();
            }

            LiquidityCheck();
            
            LifetimeMonths++;
            _startUpRounds = _startUpRounds > 0 ? _startUpRounds - 1 : 0;
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            SalesLastMonth = SalesInMonth;
            SalesInMonth = 0;
            Academy.Instance.StatsRecorder.Add("Company/LastSales", SalesLastMonth);
            Academy.Instance.StatsRecorder.Add("Company/CurrentSales", SalesInMonth);
            //Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)Liquidity);
            
            Academy.Instance.StatsRecorder.Add("Product/Price", (float)ProductPrice);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
            
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _workers.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            Academy.Instance.StatsRecorder.Add("Labor/Wage", (float)WageRate);
            Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
            Academy.Instance.StatsRecorder.Add("Company/Reward", (float)CumulatedReward);
            
            UpdateCanvasText(false);

            if (CumulatedReward >= 1_000_000)
            {
                SetReward((float)CumulatedReward * 1.5F);
                _emergencyRounds = 0;
                _startUpRounds = 12;
                EndEpisode();
            }
            else if (_emergencyRounds == 12 && _startUpRounds == 0)
            {
                for (int i = _workers.Count - 1; i >= 0; i--)
                {
                    var worker = _workers[i];
                    worker.Fire();
                    _workers.Remove(worker);
                }

                int liquidateStock = (int) Math.Floor(ProductStock / (double) ServiceLocator.Instance.Companys.Count);
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.ProductStock += liquidateStock;
                    ProductStock -= liquidateStock;
                }
                
                SetReward((float)CumulatedReward / 10 - 100_000);
                WageRate = RealwageRate * 1.5;
                _emergencyRounds = 0;
                LifetimeMonths = 0;
                _startUpRounds = 24;
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
                ServiceLocator.Instance.LaborMarketService.Workers.Where(
                    x => x.HasJob == false && x.Money < 100).ToList();
            double societyShare = unemployed.Count > 0 ? (Liquidity - companyReserve) / unemployed.Count : 0;

            foreach (var worker in unemployed)
            {
                worker.Pay(societyShare);
            }

            Liquidity = societyShare > 0 ? companyReserve : Liquidity;
        }

        public void SignJobOffer(Worker worker)
        {
            _workers.Add(worker);
            OpenPositions--;
        }
        
        public void QuitJob(Worker worker)
        {
            _workers.Remove(worker);
            //OpenPositions++;
        }

        private bool _decisionMade;
        
        
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (IsBlocked)
            {
                _decisionMade = false;
                return;
            }
            if (ServiceLocator.Instance.FlowController.Day != 1 || _decisionMade)
            {
                _decisionMade = ServiceLocator.Instance.FlowController.Day == 1;
                return;
            }

            float newWage = (actionBuffers.ContinuousActions[0] + 1) / 2 * 100 + 1;
            float newPrice = (actionBuffers.ContinuousActions[1] + 1 ) / 2 * 10 + 0.01F;
            if (newPrice < 0 || newWage < 0)
            {
                Debug.LogWarning("NOPE");
            }
            int fireWorkers = actionBuffers.DiscreteActions[0];
            int openPositions = actionBuffers.DiscreteActions[1];

            WageRate = newWage;
            ProductPrice = newPrice;
            OpenPositions = openPositions;
            
            for (var i = _workers.Count - 1; i >= 0; i--)
            {
                if (fireWorkers == 0)
                {
                    break;
                }
                var worker = _workers[i];
                _workers.Remove(worker);
                worker.Fire();
                fireWorkers--;
            }

            
            //OpenPositions = _workers.Count == 0 && OpenPositions == 0 ? 1 : OpenPositions;
            ServiceLocator.Instance.FlowController.CommitDecision();
            _decisionMade = true;
            SetBuilding();
            UpdateCanvasText(false);
        }
    }
}
