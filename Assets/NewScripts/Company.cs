using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
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
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        public GameObject startupSign;
        private BoxCollider _boxCollider;
        private int currentActiveIndex = 0;
        public int id = 0;
        public int OpenPositions { get; private set; } = 0;
        private int ProductionInMonth = 0;
        private int SalesInMonth = 0;
        private int SalesLastMonth = 0;
        private const int Days = 20;
        public double CumulatedReward { get; private set; }
        public int ProductStock { get; private set; } = 0;

        public double ProductPrice { get; private set; }

        public double WageRate { get; private set; } = 0;
        public double RealwageRate { get; private set; } = 0;

        private readonly List<Worker> _workers = new();

        public int WorkersCount => _workers.Count;

        
        private readonly System.Random _rand = new();
        public int LifetimeMonths { get; private set; } = 0;

        private int Id => GetInstanceID();

        public double Liquidity { get; private set; }
        public double ProfitInMonth { get; private set; }
        //private int _initialWorkers;

        private bool _initDone;
        private bool _isTraining;
        private bool _writeToDatabase;
        public int EmergencyRounds { get; private set; }
        public int StartUpRounds { get; private set; } = 12;

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

        private bool _canvasActive = false;
        
        void Update() {  
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
                    $"{ProductPrice:0.##}", LifetimeMonths.ToString()
                }, Id);
            }
        }

        private void SetupAgent()
        {
            //ProductStock = 100;
            WageRate = 100 + (double)_rand.Next(-100, 101) / 100;
            RealwageRate = WageRate;
            ProductPrice = 0.5 + (double) _rand.Next(-10, 11) / 100;
            stageOneBuilding.SetActive(true);
            stageTwoBuilding.SetActive(false);
            stageThreeBuilding.SetActive(false);
            OpenPositions = 2;
            EmergencyRounds = 0;
            StartUpRounds = 12;
            CumulatedReward = 0;
        }

        private void SetBuilding()
        {
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

            startupSign.SetActive(StartUpRounds > 0);
            emergencySign.SetActive(EmergencyRounds > 0);
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
            ProfitInMonth += ProductPrice * amount;
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

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation((float)Liquidity);
            sensor.AddObservation(_workers.Count);
            sensor.AddObservation((float)ProductPrice);
            sensor.AddObservation(ProductStock);
            sensor.AddObservation(OpenPositions);
            sensor.AddObservation((float)ProfitInMonth);
            sensor.AddObservation((float)WageRate);
            sensor.AddObservation(SalesLastMonth);
            sensor.AddObservation(SalesInMonth);
            sensor.AddObservation(EmergencyRounds);
            sensor.AddObservation((float)RealwageRate);
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (Liquidity > _workers.Count * WageRate || _workers.Count < 2)
            {
                actionMask.SetActionEnabled(2, 0, false);
            }

            if (EmergencyRounds > 0)
            {
                //actionMask.SetActionEnabled(1, 2, false);
            }

            if (ServiceLocator.Instance.FlowController.Step == SimulationStep.StartDaysBusiness)
            {
                //actionMask.SetActionEnabled(0, 0, false);
                //actionMask.SetActionEnabled(0, 2, false);
                //actionMask.SetActionEnabled(2, 0, false);
                //actionMask.SetActionEnabled(2, 2, false);
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

        public void StartMonth()
        {
            Academy.Instance.StatsRecorder.Add("Company/LastSales", SalesLastMonth);
            Academy.Instance.StatsRecorder.Add("Company/CurrentSales", SalesInMonth);
            Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)ProfitInMonth);
            
            Academy.Instance.StatsRecorder.Add("Product/Price", (float)ProductPrice);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
            
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _workers.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            Academy.Instance.StatsRecorder.Add("Labor/Wage", (float)WageRate);

            SalesLastMonth = SalesInMonth;
            SalesInMonth = 0;
            //ProfitInMonth = 0;
            RequestDecision();
        }

        public void StartDay()
        {
            if (ProductStock < _workers.Count * 20 * 10)
            {
                ProductStock += _workers.Count * 10;
                //Debug.LogError("Produced " + _workers.Count * 10 + " Stock: " + ProductStock);
            }
            else
            {
                //Debug.LogWarning("No production. Stock: " + ProductStock);
            }
            //RequestDecision();
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
            if (ProfitInMonth is < 0 and > -0.0001)
            {
                Debug.LogWarning("Profit below Zero");
                ProfitInMonth = 0;
            }
            else if (ProfitInMonth < -0.0001)
            {
                //Debug.LogError("Profit below Zero");
                throw new Exception("Profit below Zero: " + ProfitInMonth);
            }
        }

        public void EndMonth()
        {
            if (_writeToDatabase)
            {
                var ledger = new CompanyLedger
                {
                    companyId = Id,
                    extinct = LifetimeMonths == 0,
                    month = ServiceLocator.Instance.FlowController.Month,
                    year = ServiceLocator.Instance.FlowController.Year,
                    liquidity = Liquidity,
                    profit = ProfitInMonth,
                    workers = _workers.Count,
                    price = ProductPrice,
                    wage = WageRate,
                    sales = SalesInMonth,
                    stock = ProductStock,
                    lifetime = LifetimeMonths,
                    sessionId = ServiceLocator.Instance.SessionId,
                    emergencyRounds = EmergencyRounds,
                    isStartup = StartUpRounds > 0,
                    isTraining = _isTraining
                };
                StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
            }

            LiquidityCheck();
            
            double estimatedWorkerPayments = WageRate * _workers.Count;
            Liquidity += ProfitInMonth;
            ProfitInMonth = 0;

            if (estimatedWorkerPayments > Liquidity && _workers.Count > 0)
            {
                RealwageRate = Liquidity / _workers.Count * 0.95;
                foreach (var worker in _workers)
                {
                    worker.Pay(RealwageRate);
                    Liquidity -= RealwageRate;
                    //AddReward((float)(WageRate / 100));
                }
                //LiquidityCheck();
                //AddReward((float)(Liquidity - estimatedWorkerPayments));
                //EmergencyRounds++;
                EmergencyRounds = StartUpRounds == 0 ? EmergencyRounds + 1 : 0;
            }
            else if (_workers.Count == 0 && SalesInMonth == 0)
            {
                EmergencyRounds = StartUpRounds == 0 ? EmergencyRounds + 1 : 0;
            }
            else
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
                    double companyReserve = Liquidity * 0.1;
                    double societyShare = (Liquidity - companyReserve) / 1000;
                    //AddReward((float)societyShare);
                
                    foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
                    {
                        worker.Pay(societyShare);
                    }

                    Liquidity = companyReserve;
                }

                EmergencyRounds = 0;
            }

            LiquidityCheck();
            
            //if (Liquidity > 0 && ProductStock > 0)
            //{
            //    double storageCosts = Liquidity / ProductStock * 0.9;
            //    foreach (var worker in _workers)
            //    {
            //        worker.Pay(storageCosts / _workers.Count);
            //    }
//
            //    WageRate += storageCosts / _workers.Count;
            //    Liquidity -= storageCosts;
            //    LiquidityCheck();
            //}

            //if (SalesInMonth == 0 && SalesLastMonth == 0 && (_workers.Count == 0 || Liquidity == 0))
            //{
            //    EmergencyRounds++;
            //}
            //else
            //{
            //    EmergencyRounds = 0;
            //}
            
            LifetimeMonths++;
            StartUpRounds = StartUpRounds > 0 ? StartUpRounds - 1 : 0;
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            SetBuilding();
            UpdateCanvasText(false);

            if (CumulatedReward >= 1_000_000)
            {
                SetReward((float)CumulatedReward * 1.5F);
                Academy.Instance.StatsRecorder.Add("Company/Reward", (float)CumulatedReward);
                Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
                EmergencyRounds = 0;
                EndEpisode();
            }
            else if (EmergencyRounds == 12 && StartUpRounds == 0)
            {
                Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
                Academy.Instance.StatsRecorder.Add("Company/Reward", (float)CumulatedReward);
                for (int i = _workers.Count - 1; i >= 0; i--)
                {
                    var worker = _workers[i];
                    worker.Fire();
                    _workers.Remove(worker);
                }
                
                SetReward((float)CumulatedReward / 10 - 100_000);
                WageRate = RealwageRate * 1.5;
                EmergencyRounds = 0;
                LifetimeMonths = 0;
                EndEpisode();
            }
            //yield return new WaitForFixedUpdate();
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
        
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (ServiceLocator.Instance.FlowController.Step != SimulationStep.StartMonthBusiness)
            {
                Debug.LogError("");
            }
            // monthly decisions
            //float wageChangeRate = (actionBuffers.ContinuousActions[0] + 1) / 2 * 1.5F + 0.5F;
            //float wageChangeRate = ScaleAction(actionBuffers.ContinuousActions[0], 0.5F, 2);
            //float priceChangeRate = (actionBuffers.ContinuousActions[1] + 1) / 2 * 1.5F + 0.5F;
            //float workerCountChangeRate = actionBuffers.ContinuousActions[2];

            int wageChangeRate = actionBuffers.DiscreteActions[0];
            int priceChangeRate = actionBuffers.DiscreteActions[1];
            int workerChangeRate = actionBuffers.DiscreteActions[2];

            WageRate = wageChangeRate == 0 ? WageRate * 0.99 : wageChangeRate == 2 ? WageRate * 1.01 : WageRate;
            WageRate = WageRate < 10 ? 10 : WageRate > 100_000 ? 100_000 : WageRate;

            double newPrice = priceChangeRate == 0 ? ProductPrice * 0.99 : priceChangeRate == 2 ? ProductPrice * 1.01 : ProductPrice;
            ProductPrice = newPrice;
            ProductPrice = ProductPrice < 0.05 ? 0.05 : ProductPrice > 1000 ? 1000 : ProductPrice;
            int lastworkers = _workers.Count;
            
            if (workerChangeRate == 2)
            {
                OpenPositions = (int) Math.Ceiling(_workers.Count * 0.1);
                OpenPositions = OpenPositions == 0 ? 1 : OpenPositions;
            }
            else if (workerChangeRate == 0 && _workers.Count > 1)
            {
                int fireWorkers = (int)Math.Floor(_workers.Count * 0.1);
                if (fireWorkers > _workers.Count)
                {
                    OpenPositions = 0;
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
                }
            }

            if (lastworkers > _workers.Count * 2)
            {
                Debug.LogError("");
            }
            OpenPositions = _workers.Count == 0 && OpenPositions == 0 ? 1 : OpenPositions;
            ServiceLocator.Instance.FlowController.CommitDecision();
            SetBuilding();
            UpdateCanvasText(false);
            
        }
    }
}
