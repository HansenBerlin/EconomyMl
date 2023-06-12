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
        private int Id => GetInstanceID();
        private float Capital;
        //public float Count;
        //public float Price;
        //public float Capacity;
        private int LastSales;
        private int TotalSales;
        private int WorkerCount => ServiceLocator.Instance.LaborMarketService.CompanyWorkerCount(Id);
        private int ResourceStock;
        private float LastCapital;
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
        //private int _ressourceCapacityModifier;
        private int _workerCapacityModifier;
        private float _energyCostPerUnit;
        private float _storageCostPerUnit;
        private ProductTemplate _template;

        private int ExtinctPenaltyRounds { get; set; }
        public bool HasPenalty { get; private set; }

        public void Init(ProductTemplate productTemplate)
        {
            //Id = GetInstanceID();
            ProducedProduct = productTemplate.Product;
            Capital = productTemplate.StartCapital;
            //Type = ProducedProduct.ProductTypeOutput;
            _template = productTemplate;

            _workerPayment = productTemplate.WorkerSalary;
            _storageCostPerUnit = 0;
            _energyCostPerUnit = 0;
            _workerCapacityModifier = productTemplate.UnitsPerWorker;
            var workerCount = 333;
            ServiceLocator.Instance.LaborMarketService.Hire(workerCount, Id);
            TypeText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.ProductTypeOutput.ToString();
        }

        public float BuyFromCompany()
        {
            ProducedProduct.Amount--;
            LastSales++;
            TotalSales++;
            Capital += ProducedProduct.Price;
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";;
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
                //var maxByRessource = ResourceStock / _ressourceCapacityModifier;
                var maxByRessource = ResourceStock;
                var maxByWorkers = WorkerCount * _workerCapacityModifier;
                capacity = maxByRessource > maxByWorkers ? maxByWorkers : maxByRessource;
            }
            //CapacityText.GetComponent<TextMeshProUGUI>().text = capacity.ToString();
            return capacity;
        }

        public void RunManual()
        {
            
            LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            
            if (ExtinctPenaltyRounds == 0)
            {
                if (ProducedProduct.Amount == 0)
                {
                    ServiceLocator.Instance.LaborMarketService.Hire(1, Id);
                }
                else if (WorkerCount > 1 && ProducedProduct.Amount > _template.AverageConsumption && Capital < LastCapital)
                {
                    ServiceLocator.Instance.LaborMarketService.Fire(1, Id);
                }

                if (ProducedProduct.ProductTypeOutput == ProductType.Luxury)
                {
                    Debug.Log("");
                }
                if (Capital > 0)
                {
                    int amount = WorkerCount * _workerCapacityModifier;
                    BuyRessourceDecision(Capital, amount);
                }
                if (ProducedProduct.Amount > ProductionCapacity && Capital < LastCapital)
                {
                    ProducedProduct.Price *= 0.99F;
                }
                if (ProducedProduct.Amount < ProductionCapacity && Capital < LastCapital)
                {
                    ProducedProduct.Price *= 1.01F;
                }
                Produce();
            }
            LastSales = 0;
            LastCapital = Capital;


            if (ExtinctPenaltyRounds == 0 && Capital <= 0)
            {
                ExtinctPenaltyRounds = 21;
                ExtinctStatus.GetComponent<TextMeshProUGUI>().text = $"EXTINCT: {ProducedProduct.Price:0.##}/{WorkerCount}";
                ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
                ResourceStock = 0;
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount / 2, Id);

            }
            if (ExtinctPenaltyRounds > 0)
            {
                ExtinctPenaltyRounds--;
                if (ExtinctPenaltyRounds == 0)
                {
                    ExtinctStatus.GetComponent<TextMeshProUGUI>().text = "RUNNING";
                    ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(0, 1, 0.6F);
                    Capital += _template.StartCapital / 2;
                    ProducedProduct.Price = _template.DefaultPrice;
                }
            }
            //ProducedProduct.Price += (float)new System.Random().Next(-5, 6) / 500;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
        }

        private int count = 0;

        public void RequestNextStep()
        {
            count++;
            ExtinctStatus.GetComponent<TextMeshProUGUI>().text = "RUNNING " + count;
            ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(0, 1, 0.6F);
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{LastCapital:0}/{Capital:0}";



            //Price = ProducedProduct.Price;
            //Count = ProducedProduct.Amount;
            LastCapital = Capital;
            LastSales = 0;
            //Capacity = ProductionCapacity;

            if (Capital <= 0)
            {
                SetReward(-1F);
                ExtinctStatus.GetComponent<TextMeshProUGUI>().text = $"EXTINCT: {ProducedProduct.Price:0.##}/{WorkerCount}";
                ExtinctStatus.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);

                //Capital = _template.StartCapital / 2;
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount - 1, Id);
                //ServiceLocator.Instance.LaborMarketService.Hire(1);
                var lowestPrice =
                    ServiceLocator.Instance.ProductMarketService.LowestPrice(ProducedProduct.ProductTypeOutput);
                ProducedProduct.Price = lowestPrice * 0.95F;
                //ResourceStock = 0;
                //LastSales = 0;
                //TotalSales = 0;
                ExtinctPenaltyRounds = 10;
                HasPenalty = true;
                CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
                PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
                StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
                CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
                WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
                LastSalesText.GetComponent<TextMeshProUGUI>().text = LastSales.ToString();
                TotalSalesText.GetComponent<TextMeshProUGUI>().text = TotalSales.ToString();
                EndEpisode();
                return;
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
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount / 4, Id);

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
                return;
                //Destroy(this);
            }
            
            RequestDecision();
            
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(Capital);
            sensor.AddObservation(LastSales);
            sensor.AddObservation(ProductionCapacity);
            sensor.AddObservation(_workerPayment*WorkerCount);
            //sensor.AddObservation(_energyCostPerUnit*ProductionCapacity);
            //sensor.AddObservation(_storageCostPerUnit*ProducedProduct.Amount);
            sensor.AddObservation(ProducedProduct.Price);
            sensor.AddObservation(ProducedProduct.Amount);
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput));
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeOutput));
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (HasPenalty)
            {
                throw new Exception("NOT ALLOWED");
            }
            int buyResources = actionBuffers.DiscreteActions[0];
            if (buyResources == 1 && Capital > 0)
            {
                float maxFromCapital = actionBuffers.DiscreteActions[1] + 1;
                BuyRessourceDecision(Capital / maxFromCapital, WorkerCount * _workerCapacityModifier);
            }

            int hireOrFire = actionBuffers.DiscreteActions[2];
            int count = actionBuffers.DiscreteActions[3];
            
            if (hireOrFire == 1)
            {
                ServiceLocator.Instance.LaborMarketService.Hire(count, Id);
            }
            else if (hireOrFire == 2 && WorkerCount > count)
            {
                ServiceLocator.Instance.LaborMarketService.Fire(count, Id);
            }
            
            int produce = actionBuffers.DiscreteActions[4];
            Produce();
            if (produce == 1)
            {
            }

            int priceChange = actionBuffers.DiscreteActions[5];
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
        
        public void DecreasePenalty()
        {
            Produce();
            var lowestPrice =
                ServiceLocator.Instance.ProductMarketService.LowestPrice(ProducedProduct.ProductTypeOutput);
            ProducedProduct.Price = lowestPrice < ProducedProduct.Price ? lowestPrice * 0.95F : ProducedProduct.Price;
            
            ExtinctPenaltyRounds--;
            if (ExtinctPenaltyRounds == 0)
            {
                Capital = Capital < 0 ? 0 : Capital;
                Capital += _template.StartCapital / 2;
                //var averagePrice =
                //    ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeOutput);
                //ProducedProduct.Price = (_template.DefaultPrice + averagePrice) / 2;
                ProducedProduct.Price = _template.DefaultPrice;
                HasPenalty = false;
            }
            
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            
        }

        private void Produce()
        {
            //MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
            if (ProductionCapacity > _template.UnitsPerWorker * 10)
            {
                AddReward(0.001F);
            }
            
            ProducedProduct.Amount += ProductionCapacity;
            Capital -= ProductionCapacity * _energyCostPerUnit;
            if (ProducedProduct.ProductTypeInput != ProductType.None)
            {
                ResourceStock -= ProductionCapacity;
            }
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
        }

        private void BuyRessourceDecision(float maxSpending, int amount)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }

            float avgMarketPrice = ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput);
            if (avgMarketPrice + _template.WorkerSalary / (float)_template.UnitsPerWorker > ProducedProduct.Price)
            {
                return;
            }
            var receipt = ServiceLocator.Instance.ProductMarketService
                .Buy(ProducedProduct.ProductTypeInput, amount, maxSpending);
            
            Capital -= receipt.AmountPaid;
            ResourceStock += receipt.CountBought;
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }
    }
}