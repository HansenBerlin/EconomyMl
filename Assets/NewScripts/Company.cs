using System;
using System.Collections.Generic;
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
        public int Id => GetInstanceID();
        public float Capital;
        //public float Count;
        //public float Price;
        //public float Capacity;
        public int LastSales;
        public int TotalSales;
        private int WorkerCount => ServiceLocator.Instance.LaborMarketService.CompanyWorkerCount(Id);
        public int ResourceStock;
        public float LastCapital;
        //public ProductType Type;
        public TextMeshProUGUI TypeText;
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        public TextMeshProUGUI StockText;
        public TextMeshProUGUI MaterialText;
        public TextMeshProUGUI LastSalesText;
        public TextMeshProUGUI TotalSalesText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI CapacityText;
        public TextMeshProUGUI ExtinctStatus;
        
        public int ProductionCapacity => CalculateProductionCapacity();
        public Product ProducedProduct { get; set; }

        //private ProductMarket _productMarket;
        //private LaborMarket _laborMarket;
        private int _workerPayment;
        private int _ressourceCapacityModifier;
        private int _workerCapacityModifier;
        private float _energyCostPerUnit;
        private float _storageCostPerUnit;

        public int ExtinctPenaltyRounds { get; set; }

        public void Init(Product produced)
        {
            //Id = GetInstanceID();
            ProducedProduct = produced;
            //Type = ProducedProduct.ProductTypeOutput;
            ServiceLocator.Instance.ProductMarketService.payCompanyEvent.AddListener((x) =>
            {
                if (int.Parse(x) == Id)
                {
                    ProducedProduct.Amount--;
                    LastSales++;
                    Capital += ProducedProduct.Price;
                    Debug.Log("Sold!");
                }
            });
            _workerPayment = produced.ProductTypeOutput == ProductType.Food ? 25 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 35 : 45;
            _storageCostPerUnit = produced.ProductTypeOutput == ProductType.Food ? 0.0F :
                produced.ProductTypeOutput == ProductType.Intermediate ? 0.0F : 0F;
            _energyCostPerUnit = produced.ProductTypeOutput == ProductType.Food ? 0.1F :
                produced.ProductTypeOutput == ProductType.Intermediate ? 0.2F : 0.5F;
            _workerCapacityModifier = produced.ProductTypeOutput == ProductType.Food ? 32 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 16 : 4;
            _ressourceCapacityModifier = produced.ProductTypeOutput == ProductType.Luxury ? 5 : 1;
            var workerCount = produced.ProductTypeOutput == ProductType.Food ? 104 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 104 : 83;
            ServiceLocator.Instance.LaborMarketService.Hire(workerCount, Id);
            TypeText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.ProductTypeOutput.ToString();
        }

        public float BuyFromCompany()
        {
            ProducedProduct.Amount--;
            LastSales++;
            TotalSales++;
            Capital += ProducedProduct.Price;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";;
            LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
            TotalSalesText.GetComponent<TextMeshProUGUI>().text = TotalSales.ToString();
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();

            return ProducedProduct.Price;
            //Debug.Log("Sold!");
        }

        private int CalculateProductionCapacity()
        {
            int capacity = 0;
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                capacity = WorkerCount * _workerCapacityModifier;
            }
            else
            {
                var maxByRessource = ResourceStock / _ressourceCapacityModifier;
                var maxByWorkers = WorkerCount * _workerCapacityModifier;
                capacity = maxByRessource > maxByWorkers ? maxByWorkers : maxByRessource;
            }
            CapacityText.GetComponent<TextMeshProUGUI>().text = capacity.ToString();
            return capacity;
        }

        public void RunManual()
        {
            if (Capital > LastCapital && ExtinctPenaltyRounds == 0)
            {
                ServiceLocator.Instance.LaborMarketService.Hire(1, Id);
            }
            else if (WorkerCount > 1)
            {
                ServiceLocator.Instance.LaborMarketService.Fire(1, Id);
            }

            if (ProducedProduct.Amount > ProductionCapacity)
            {
                ProducedProduct.Price *= 0.99F;
            }
            if (ProducedProduct.Amount < ProductionCapacity && Capital > LastCapital)
            {
                ProducedProduct.Price *= 1.01F;
            }
            LastSales = 0;
            LastCapital = Capital;
            LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();


            if (Capital > 0 && ExtinctPenaltyRounds == 0)
            {
                int amount = WorkerCount * _workerCapacityModifier * _ressourceCapacityModifier;
                BuyRessourceDecision(Capital, amount);
                Produce();
            }
            else if (ExtinctPenaltyRounds == 0)
            {
                ExtinctPenaltyRounds = 21;
                ExtinctStatus.GetComponent<TextMeshProUGUI>().text = $"EXTINCT: {ProducedProduct.Price:0.##}/{WorkerCount}";
                ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
                ResourceStock = 0;
            }
            if (ExtinctPenaltyRounds > 0)
            {
                ExtinctPenaltyRounds--;
                if (ExtinctPenaltyRounds == 0)
                {
                    ExtinctStatus.GetComponent<TextMeshProUGUI>().text = "RUNNING";
                    ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(0, 1, 0.6F);
                    Capital += ProducedProduct.ProductTypeOutput == ProductType.Luxury 
                        ? 40000 : ProducedProduct.ProductTypeOutput == ProductType.Intermediate 
                            ? 20000 : 15000;
                    ProducedProduct.Price = ProducedProduct.ProductTypeOutput == ProductType.Food ? 1 :
                        ProducedProduct.ProductTypeOutput == ProductType.Intermediate ? 2.5F : 25;
                }
            }
            //ProducedProduct.Price += (float)new System.Random().Next(-5, 6) / 500;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
        }


        public void RequestNextStep()
        {
            ExtinctStatus.GetComponent<TextMeshProUGUI>().text = "RUNNING";
            ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(0, 1, 0.6F);


            //Price = ProducedProduct.Price;
            //Count = ProducedProduct.Amount;
            LastCapital = Capital;
            LastSales = 0;
            //Capacity = ProductionCapacity;

            if (Capital <= 0)
            {
                SetReward(-5F);
                ExtinctStatus.GetComponent<TextMeshProUGUI>().text = $"EXTINCT: {ProducedProduct.Price:0.##}/{WorkerCount}";
                ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
                
                Capital = ProducedProduct.ProductTypeOutput == ProductType.Luxury 
                    ? 80000 : ProducedProduct.ProductTypeOutput == ProductType.Intermediate 
                        ? 40000 : 30000;
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount, Id);
                ServiceLocator.Instance.LaborMarketService.Hire(2, Id);
                //ServiceLocator.Instance.LaborMarketService.Hire(1);
                ProducedProduct.Price = ProducedProduct.ProductTypeOutput == ProductType.Food ? 1 :
                    ProducedProduct.ProductTypeOutput == ProductType.Intermediate ? 2.5F : 25;
                ProducedProduct.Amount = 0;
                ResourceStock = 0;
                LastSales = 0;
                TotalSales = 0;
                ExtinctPenaltyRounds = 200;
                CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
                PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
                StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
                CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
                WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
                LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
                TotalSalesText.GetComponent<TextMeshProUGUI>().text = TotalSales.ToString();
                EndEpisode();
                //Destroy(this);
            }
            if (Capital > 10000000)
            {
                SetReward(2F);
                Capital /= 4;
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount / 4, Id);
                //ServiceLocator.Instance.LaborMarketService.Hire(1);
                ProducedProduct.Amount /= 4;
                ResourceStock = ResourceStock > 0 ? ResourceStock / 4 : 0;
                
                ExtinctPenaltyRounds = 50;
                ExtinctStatus.GetComponent<TextMeshProUGUI>().text = "WON";
                ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 1);
                CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
                PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
                StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
                CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
                WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
                LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
                TotalSalesText.GetComponent<TextMeshProUGUI>().text = TotalSales.ToString();
                EndEpisode();
                //Destroy(this);
            }
            
            RequestDecision();
            
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(Capital);
            sensor.AddObservation(LastSales);
            sensor.AddObservation(ProductionCapacity);
            sensor.AddObservation(WorkerCount);
            sensor.AddObservation(_workerPayment*WorkerCount);
            sensor.AddObservation(_energyCostPerUnit*ProductionCapacity);
            sensor.AddObservation(_storageCostPerUnit*ProducedProduct.Amount);
            sensor.AddObservation(ProducedProduct.Price);
            sensor.AddObservation(ProducedProduct.Amount);
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput));
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeOutput));
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int buyResources = actionBuffers.DiscreteActions[0];
            if (buyResources == 1 && Capital > 0)
            {
                int amount = actionBuffers.DiscreteActions[1] + 1;
                float maxFromCapital = actionBuffers.DiscreteActions[2] + 1;
                BuyRessourceDecision(Capital / maxFromCapital, amount);
            }

            int hireOrFire = actionBuffers.DiscreteActions[4];
            int count = actionBuffers.DiscreteActions[5];
            
            if (hireOrFire == 1)
            {
                ServiceLocator.Instance.LaborMarketService.Hire(count, Id);
            }
            else if (hireOrFire == 2 && WorkerCount > count)
            {
                ServiceLocator.Instance.LaborMarketService.Fire(count, Id);
            }
            
            int produce = actionBuffers.DiscreteActions[3];
            if (produce == 1)
            {
                Produce();
            }

            int priceChange = actionBuffers.DiscreteActions[6];
            SetPriceDecision(priceChange);
        }

        private void SetPriceDecision(int priceChange)
        {
            float newPrice = priceChange == 0 ? ProducedProduct.Price * 0.99F :
                priceChange == 2 ? ProducedProduct.Price * 1.01F : ProducedProduct.Price;
            newPrice = newPrice < 0.1F ? 0.1F : newPrice;
            ProducedProduct.Price = newPrice;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
        }

        public void EndRound()
        {
            int workerPayments = _workerPayment * WorkerCount;
            Capital -= workerPayments;
            ServiceLocator.Instance.LaborMarketService.Pay(_workerPayment, Id);
            Capital -= _storageCostPerUnit * ProducedProduct.Amount;
            AddReward(Capital > LastCapital ? 0.01F : -0.001F);
        }

        private void Produce()
        {
            ProducedProduct.Amount += ProductionCapacity;
            if (ProductionCapacity > 1)
            {
                AddReward(0.001F);
            }
            Capital -= ProductionCapacity * _energyCostPerUnit;
            if (ProducedProduct.ProductTypeInput != ProductType.None)
            {
                ResourceStock -= ProductionCapacity * _ressourceCapacityModifier;
            }
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }

        private void BuyRessourceDecision(float maxSpending, int amount)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }

            float avgMarketPrice = ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput);
            if (avgMarketPrice * _ressourceCapacityModifier > ProducedProduct.Price)
            {
                return;
            }
            var receipt = ServiceLocator.Instance.ProductMarketService
                .Buy(ProducedProduct.ProductTypeInput, amount, maxSpending);
            
            Capital -= receipt.AmountPaid;
            ResourceStock += receipt.CountBought;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }
    }
}