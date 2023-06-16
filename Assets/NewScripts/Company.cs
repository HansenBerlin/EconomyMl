﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        private int currentActiveIndex = 0;
        public int id = 0;
        public int OpenPositions { get; private set; } = 0;
        private int ProductionInMonth = 0;
        private int SalesInMonth = 0;
        private int SalesLastMonth = 0;
        public int ProductStock { get; private set; } = 0;

        public decimal ProductPrice { get; private set; }

        public decimal WageRate { get; private set; } = 0;

        private readonly List<Worker> _workers = new();

        private decimal PercentageSoldLastMonth = 0;
        private decimal PercentageSold = 0;
        private readonly System.Random _rand = new(42);
        public int LifetimeMonths { get; private set; } = 0;

        public ActionPhase CurrentPhase;
        
        
        private int Id => GetInstanceID();

        public decimal Liquidity { get; private set; }
        public decimal ProfitInMonth { get; private set; }
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        private int initialWorkers;

        private bool _initDone;
        public int IsOnEmergencySaleRounds { get; private set; }

        public void Init(int companiesPerType)
        {
            id = Id;
            initialWorkers = (int)Math.Floor(1000 / (decimal)companiesPerType);
            Liquidity = 79000 / (decimal)companiesPerType;
            var availableWorkers = ServiceLocator.Instance.LaborMarketService.Workers.Where(x => x.HasJob == false).ToList();
            for (int i = 0; i < initialWorkers; i++)
            {
                availableWorkers[i].InitialJobSetup(this);
                _workers.Add(availableWorkers[i]);
            }
            WorkersText.GetComponent<TextMeshProUGUI>().text = _workers.Count.ToString();
            SetupAgent();
            LifetimeMonths = 1;
            emergencySign.SetActive(false);
            _initDone = true;
        }

        private void SetupAgent()
        {
            //ProductStock = 100;
            WageRate = 21 + (decimal)_rand.NextDouble() - 0.1M;
            ProductPrice = 1 + (decimal) _rand.Next(-100, 101) / 100;
            stageOneBuilding.SetActive(true);
            stageTwoBuilding.SetActive(false);
            stageThreeBuilding.SetActive(false);
        }

        private void SetBuilding()
        {
            var buildings = new List<GameObject>
            {
                stageOneBuilding, stageTwoBuilding, stageThreeBuilding
            };
            decimal companycount = ServiceLocator.Instance.Companys.Count;
            decimal personCount = ServiceLocator.Instance.LaborMarketService.Workers.Count;
            decimal ratio = personCount / companycount;
            int activeIndex = _workers.Count < ratio ? 0 : _workers.Count > ratio * companycount / 4 ? 2 : 1;
            if (activeIndex != currentActiveIndex)
            {
                for (var i = 0; i < buildings.Count; i++)
                {
                    buildings[i].SetActive(i == activeIndex);
                }

                currentActiveIndex = activeIndex;
            }
        }

        public override void OnEpisodeBegin()
        {
            if (_initDone)
            {
                //ProductStock = 50;
                WageRate = 21 + (decimal) _rand.NextDouble() - 0.1M;
                ProductPrice = 1 + (decimal) _rand.Next(-100, 101) / 100;
                OpenPositions = initialWorkers / 2;
                emergencySign.SetActive(false);
                IsOnEmergencySaleRounds = 0;
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
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Liquidity:0}";
            
            AddReward(amount * 0.0001F);

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
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (Liquidity > _workers.Count * WageRate || _workers.Count < 2)
            {
                actionMask.SetActionEnabled(2, 0, false);
            }

            if (IsOnEmergencySaleRounds > 0)
            {
                actionMask.SetActionEnabled(1, 2, false);
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
            ProfitInMonth = 0;
            CurrentPhase = ActionPhase.BeginMonth;
            RequestDecision();
        }

        public void StartDay()
        {
            CurrentPhase = ActionPhase.BeginDay;
            ProductStock += _workers.Count * 1;
        }

        private void LiquidityCheck()
        {
            if (Liquidity is < 0 and > -0.0001M)
            {
                Debug.LogWarning("Liquidity below Zero");
                Liquidity = 0;
            }
            else if (Liquidity < -0.0001M)
            {
                throw new Exception("Liquidity below Zero");
            }
            if (ProfitInMonth is < 0 and > -0.0001M)
            {
                Debug.LogWarning("Profit below Zero");
                ProfitInMonth = 0;
            }
            else if (ProfitInMonth < -0.0001M)
            {
                throw new Exception("Profit below Zero");
            }
        }

        public void EndMonth()
        {
            if (ServiceLocator.Instance.FlowController.Month == 12)
            {
                Debug.Log("  ");
            }
            var ledger = new CompanyLedger
            {
                companyId = Id,
                extinct = LifetimeMonths == 0,
                month = ServiceLocator.Instance.FlowController.Month,
                year = ServiceLocator.Instance.FlowController.Year,
                liquidity = (double)Liquidity,
                profit = (double)ProfitInMonth,
                workers = _workers.Count,
                wage = (double)WageRate,
                sales = SalesInMonth,
                stock = ProductStock,
                lifetime = LifetimeMonths,
                sessionId = ServiceLocator.Instance.SessionId,
                emergencyRounds = IsOnEmergencySaleRounds
            };
            
            StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));

            LiquidityCheck();
            AddReward((float)LifetimeMonths / 1000);
            if (SalesInMonth == 0)
            {
                AddReward(-0.02F);
            }
            if (SalesInMonth > SalesLastMonth)
            {
                AddReward(0.1F);   
            }
           
            CurrentPhase = ActionPhase.EndMonth;
            decimal estimatedWorkerPayments = WageRate * _workers.Count;

            if (estimatedWorkerPayments < Liquidity + ProfitInMonth && estimatedWorkerPayments > Liquidity)
            {
                decimal diff = (Liquidity - estimatedWorkerPayments) * -1;
                Liquidity += diff;
                ProfitInMonth -= diff;
                LiquidityCheck();
            }
            else if (estimatedWorkerPayments > Liquidity + ProfitInMonth)
            {
                WageRate = (ProfitInMonth + Liquidity) / _workers.Count * 0.95M;
                Liquidity += ProfitInMonth;
                ProfitInMonth = 0;
                AddReward(-0.1F);
                LiquidityCheck();
            }
            
            foreach (var worker in _workers)
            {
                worker.Pay(WageRate);
                Liquidity -= WageRate;
            }
            LiquidityCheck();
            
            if (ProfitInMonth > (_workers.Count + 1) * WageRate / 12)
            {
                decimal companyReserve = ProfitInMonth * 0.8M;
                ProfitInMonth -= companyReserve;
                Liquidity += companyReserve;
                
                decimal societyShare = ProfitInMonth / 1000;
                AddReward((float)societyShare);
                foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
                {
                    worker.Pay(societyShare);
                }

                ProfitInMonth = 0;
            }
            else if (ProfitInMonth > 0)
            {
                Liquidity += ProfitInMonth;
                ProfitInMonth = 0;
            }
            
            LiquidityCheck();
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Liquidity:0.##}";
            SetBuilding();
            
            if (Liquidity == 0 && ProductStock > SalesLastMonth + SalesInMonth && _workers.Count == 0)
            {
                if (IsOnEmergencySaleRounds == 6)
                {
                    SetReward(-1F);
                    Academy.Instance.StatsRecorder.Add("Company/Extinctions", LifetimeMonths);
                    for (int i = _workers.Count - 1; i >= 0; i--)
                    {
                        var worker = _workers[i];
                        worker.Fire();
                        _workers.Remove(worker);
                    }
                    LifetimeMonths = 0;
                    EndEpisode();
                }
                else
                {
                    AddReward(-0.05F);
                    ProductPrice *= 0.95M;
                    IsOnEmergencySaleRounds++;
                    LifetimeMonths++;
                    emergencySign.SetActive(true);
                }
            }
            else
            {
                LifetimeMonths++;
                IsOnEmergencySaleRounds = 0;
                emergencySign.SetActive(false);
            }

            //yield return new WaitForFixedUpdate();
        }

        public void SignJobOffer(Worker worker)
        {
            _workers.Add(worker);
            OpenPositions--;
            WorkersText.GetComponent<TextMeshProUGUI>().text = _workers.Count.ToString();
        }
        
        public void QuitJob(Worker worker)
        {
            _workers.Remove(worker);
            WorkersText.GetComponent<TextMeshProUGUI>().text = _workers.Count.ToString();
            //OpenPositions++;
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // monthly decisions
            //float wageChangeRate = (actionBuffers.ContinuousActions[0] + 1) / 2 * 1.5F + 0.5F;
            //float wageChangeRate = ScaleAction(actionBuffers.ContinuousActions[0], 0.5F, 2);
            //float priceChangeRate = (actionBuffers.ContinuousActions[1] + 1) / 2 * 1.5F + 0.5F;
            //float workerCountChangeRate = actionBuffers.ContinuousActions[2];

            int wageChangeRate = actionBuffers.DiscreteActions[0];
            int priceChangeRate = actionBuffers.DiscreteActions[1];
            int workerChangeRate = actionBuffers.DiscreteActions[2];

            WageRate = wageChangeRate == 0 ? WageRate * 0.95M : wageChangeRate == 2 ? WageRate * 1.05M : WageRate;
            if (WageRate <= 1)
            {
                WageRate = 1;
            }
            decimal newPrice = priceChangeRate == 0 ? ProductPrice * 0.95M : priceChangeRate == 2 ? ProductPrice * 1.05M : ProductPrice;
            ProductPrice = newPrice < 0.1M ? 0.1M : newPrice;
            if (workerChangeRate == 2)
            {
                OpenPositions = (int) Math.Ceiling(_workers.Count * 1.1);
                OpenPositions = OpenPositions == 0 ? 1 : OpenPositions;
            }
            else if (workerChangeRate == 0 && _workers.Count > 1)
            {
                int fireWorkers = (int)Math.Floor(_workers.Count * 1.1);
                if (fireWorkers > 1)
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

                    WorkersText.GetComponent<TextMeshProUGUI>().text = _workers.Count.ToString();
                }
            }

            OpenPositions = _workers.Count == 0 && OpenPositions == 0 ? 1 : OpenPositions;
        }
    }
}