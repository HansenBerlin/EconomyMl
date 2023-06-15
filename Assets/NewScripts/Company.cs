using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
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
        public int id = 0;
        public int OpenPositions = 0;
        private int ProductionInMonth = 0;
        private int SalesInMonth = 0;
        private int SalesLastMonth = 0;
        public int ProductStock { get; private set; } = 0;
        public float ProductPrice = 0;
        public float WageRate { get; private set; } = 0;
        public float ReserveAmount { get; private set; } = 0;

        private readonly List<Worker> _workers = new();

        private float PercentageSoldLastMonth = 0;
        private float PercentageSold = 0;
        private readonly System.Random _rand = new(42);
        public int LifetimeMonths { get; private set; } = 0;

        public ActionPhase CurrentPhase;
        
        
        private int Id => GetInstanceID();

        public float Liquidity;
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        

        private bool _initDone;

        public void Init()
        {
            id = Id;
            var availableWorkers = ServiceLocator.Instance.LaborMarketService.Workers.Where(x => x.HasJob == false).ToList();
            for (int i = 0; i < 9; i++)
            {
                availableWorkers[i].InitialJobSetup(this);
                _workers.Add(availableWorkers[i]);
            }
            WorkersText.GetComponent<TextMeshProUGUI>().text = _workers.Count.ToString();
            SetupAgent();
            _initDone = true;
        }

        private void SetupAgent()
        {
            ProductStock = 100;
            WageRate = 25 + (float) _rand.NextDouble() - 0.1F;
            ProductPrice = 1 + (float) _rand.Next(-100, 101) / 100;
            
        }

        public override void OnEpisodeBegin()
        {
            if (_initDone)
            {
                ProductStock = 50;
                WageRate = 25 + (float) _rand.NextDouble() - 0.1F;
                ProductPrice = 1 + (float) _rand.Next(-100, 101) / 100;
                OpenPositions = 5;
            }
        }

        public Receipt BuyFromCompany(int amount)
        {
            amount = amount > ProductStock ? ProductStock : amount;
            ProductStock -= amount;
            SalesInMonth += amount;
            
            if (ProductPrice < 0)
            {
                throw new Exception("Less than zero.");
            }
            Liquidity += ProductPrice * amount;
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Liquidity:0}";
            
            AddReward(amount * 0.0001F);

            return new Receipt
            {
                AmountPaid = amount * ProductPrice,
                CountBought = amount
            };
        }

        public void EndCompanyEpisode()
        {
            
            //EndEpisode();
        }


        
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(Liquidity);
            sensor.AddObservation(_workers.Count);
            sensor.AddObservation(ProductPrice);
            sensor.AddObservation(ProductStock);
            sensor.AddObservation(OpenPositions);
            sensor.AddObservation(ReserveAmount);
            sensor.AddObservation(WageRate);
            sensor.AddObservation(SalesLastMonth);
            sensor.AddObservation(SalesInMonth);
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (ReserveAmount > _workers.Count * WageRate || _workers.Count == 0)
            {
                actionMask.SetActionEnabled(2, 0, false);
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
            Academy.Instance.StatsRecorder.Add("Company/Reserve", ReserveAmount);
            
            Academy.Instance.StatsRecorder.Add("Product/Price", ProductPrice);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStock);
            
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _workers.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            Academy.Instance.StatsRecorder.Add("Labor/Wage", WageRate);

            SalesLastMonth = SalesInMonth;
            SalesInMonth = 0;
            CurrentPhase = ActionPhase.BeginMonth;
            RequestDecision();
        }

        public void StartDay()
        {
            CurrentPhase = ActionPhase.BeginDay;
            ProductStock += _workers.Count * 2;
        }

        public void EndMonth()
        {
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", Liquidity);

            if (SalesInMonth == 0)
            {
                AddReward(-0.1F);
            }
            if (SalesInMonth > SalesLastMonth)
            {
                AddReward(0.1F);   
            }
           
            CurrentPhase = ActionPhase.EndMonth;
            if (Liquidity < _workers.Count * WageRate)
            {
                AddReward(-0.01F);
                if (ReserveAmount + Liquidity < _workers.Count * WageRate)
                {
                    AddReward(-0.05F);
                    WageRate = (ReserveAmount + Liquidity) / _workers.Count;
                    foreach (var worker in _workers)
                    {
                        worker.Fire();
                        worker.Pay(WageRate);
                    }
                    _workers.Clear();
                    Liquidity = 0;
                    ReserveAmount = 0;
                    WageRate = 1;
                    if (ProductStock == 0 || SalesLastMonth == 0)
                    {
                        SetReward(-1F);
                        Academy.Instance.StatsRecorder.Add("Company/Extinctions", 1);
                        EndEpisode();
                        LifetimeMonths = 0;
                        return;
                    }
                }
                else
                {
                    float diff = _workers.Count * WageRate - Liquidity;
                    Liquidity += diff;
                    ReserveAmount -= diff;
                }
            }

            var old = Liquidity;
            
            foreach (var worker in _workers)
            {
                worker.Pay(WageRate);
                Liquidity -= WageRate;
            }
            if (Liquidity < 0)
            {
                if (Liquidity > -1)
                {
                    Liquidity = 0;
                }
                else
                {
                    throw new Exception($"{old - WageRate * _workers.Count:0.##} // {Liquidity:0.##}");
                }
            }
            
            if (Liquidity > 0)
            {
                AddReward(0.01F);
                float reserve = Liquidity * 0.5F;
                ReserveAmount += reserve;
                Liquidity -= reserve;
                if (Liquidity < 0)
                {
                    throw new Exception($"{old - WageRate * _workers.Count:0.##} // {Liquidity:0.##}");
                }
            }

            if (Liquidity > 0)
            {
                float share = Liquidity / 1000;
                AddReward(share);
                foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
                {
                    worker.Pay(share);
                }
            }

            LifetimeMonths++;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{ReserveAmount:0}";

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

            WageRate = wageChangeRate == 0 ? WageRate * 0.95F : wageChangeRate == 2 ? WageRate * 1.05F : WageRate;
            if (WageRate <= 1)
            {
                WageRate = 1;
            }
            ProductPrice = priceChangeRate == 0 ? ProductPrice * 0.95F : priceChangeRate == 2 ? ProductPrice * 1.05F : ProductPrice;
            ProductPrice = ProductPrice < 0.1F ? 0.1F : ProductPrice;
            if (workerChangeRate == 2)
            {
                OpenPositions = (int) Math.Ceiling(_workers.Count * 1.1);
                OpenPositions = OpenPositions == 0 ? 1 : OpenPositions;
            }
            else if (workerChangeRate == 0)
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
        }
    }
}