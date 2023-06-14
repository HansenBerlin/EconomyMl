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
        public int OpenPositions = 0;
        private int ProductionInMonth = 0;
        private int SalesInMonth = 0;
        private int SalesLastMonth = 0;
        public int ProductStock = 0;
        public float ProductPrice = 0;
        public float WageRate = 0;
        public float ReserveAmount = 0;

        private readonly List<Worker> _workers = new();

        private float PercentageSoldLastMonth = 0;
        private float PercentageSold = 0;
        private readonly System.Random _rand = new(42);

        public ActionPhase CurrentPhase;
        
        
        private int Id => GetInstanceID();

        public float Liquidity;
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        

        private bool _initDone;

        public void Init()
        {
            var availableWorkers = ServiceLocator.Instance.LaborMarketService.Workers.Where(x => x.HasJob == false).ToList();
            for (int i = 0; i < 9; i++)
            {
                availableWorkers[i].InitialJobSetup(this);
            }
        }

        private void SetupAgent()
        {
            ProductStock = 50;
            WageRate = 52 + (float) _rand.NextDouble() - 0.1F;
            ProductPrice = 1 + (float) _rand.Next(-100, 101) / 100;
        }

        public override void OnEpisodeBegin()
        {
            if (_initDone)
            {
                SetupAgent();
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
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Liquidity:0}";
            
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
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (CurrentPhase != ActionPhase.BeginMonth)
            {
                for (int i = 0; i < 20; i++)
                {
                    bool isEnabled = i == 9; 
                    actionMask.SetActionEnabled(0, i, isEnabled);
                    actionMask.SetActionEnabled(1, i, isEnabled);
                    actionMask.SetActionEnabled(2, i, isEnabled);
                }
            }
        }

        public void StartMonth()
        {
            SalesLastMonth = SalesInMonth;
            SalesInMonth = 0;
            CurrentPhase = ActionPhase.BeginMonth;
            RequestDecision();
        }

        public void StartDay()
        {
            CurrentPhase = ActionPhase.BeginDay;
            ProductStock += _workers.Count;
        }

        public void EndMonth()
        {
            CurrentPhase = ActionPhase.EndMonth;
            if (Liquidity < _workers.Count * WageRate)
            {
                if (ReserveAmount + Liquidity < _workers.Count * WageRate)
                {
                    WageRate = (ReserveAmount + Liquidity) / _workers.Count;
                    Liquidity += ReserveAmount;
                    ReserveAmount = 0;
                }
                else
                {
                    float diff = _workers.Count * WageRate - Liquidity;
                    Liquidity += diff;
                    ReserveAmount -= diff;
                }
            }
            foreach (var worker in _workers)
            {
                worker.Pay(WageRate);
                Liquidity -= WageRate;
            }

            if (Liquidity > 0)
            {
                float plannedReserve = _workers.Count * WageRate / 5;
                if (plannedReserve < Liquidity)
                {
                    ReserveAmount += plannedReserve;
                    Liquidity -= plannedReserve;
                }
                else
                {
                    ReserveAmount += Liquidity;
                    Liquidity = 0;
                }
            }

            if (Liquidity > 0)
            {
                float share = Liquidity / _workers.Count;
                foreach (var worker in _workers)
                {
                    worker.Pay(share);
                }
            }
        }

        public void SignJobOffer(Worker worker)
        {
            _workers.Add(worker);
            OpenPositions--;
        }
        
        public void QuitJob(Worker worker)
        {
            _workers.Remove(worker);
            OpenPositions++;
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // monthly decisions
            //float wageChangeRate = (actionBuffers.ContinuousActions[0] + 1) / 2 * 1.5F + 0.5F;
            //float wageChangeRate = ScaleAction(actionBuffers.ContinuousActions[0], 0.5F, 2);
            //float priceChangeRate = (actionBuffers.ContinuousActions[1] + 1) / 2 * 1.5F + 0.5F;
            //float workerCountChangeRate = actionBuffers.ContinuousActions[2];

            float wageChangeRate = (float)(actionBuffers.DiscreteActions[0] + 1) / 10;
            float priceChangeRate = (float)(actionBuffers.DiscreteActions[1] + 1) / 10;
            float workerChangeRate = (float)(actionBuffers.DiscreteActions[2] + 1) / 10;

            WageRate *= wageChangeRate;
            ProductPrice *= priceChangeRate;
            if (workerChangeRate > 1)
            {
                OpenPositions = (int) Math.Ceiling(_workers.Count * workerChangeRate);
            }
            else if (workerChangeRate < 1)
            {
                int fireWorkers = (int)Math.Floor(_workers.Count * workerChangeRate);
                if (workerChangeRate < 1)
                {
                    OpenPositions = 0;
                    var randomIndices = Utilitis.GenerateRandomArray(0, _workers.Count);
                    for (int i = 0; i < fireWorkers; i++)
                    {
                        var worker = _workers[randomIndices[i]];
                        worker.Fire();
                    }
                }
            }
        }
    }
}