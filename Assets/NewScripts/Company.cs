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
        
        public int ProductionCapacity => CalculateProductionCapacity();
        public Product ProducedProduct { get; set; }

        //private ProductMarket _productMarket;
        //private LaborMarket _laborMarket;
        private int _workerPayment;
        private int _ressourceCapacityModifier;
        private int _workerCapacityModifier;
        private float _energyCostPerUnit;

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
            _workerPayment = produced.ProductTypeOutput == ProductType.Food ? 35 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 35 : 60;
            _energyCostPerUnit = produced.ProductTypeOutput == ProductType.Food ? 0.2F :
                produced.ProductTypeOutput == ProductType.Intermediate ? 1 : 5;
            _workerCapacityModifier = produced.ProductTypeOutput == ProductType.Food ? 50 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 10 : 5;
            _ressourceCapacityModifier = produced.ProductTypeOutput == ProductType.Luxury ? 5 : 1;
            var workerCount = produced.ProductTypeOutput == ProductType.Food ? 2 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 5 : 2;
            ServiceLocator.Instance.LaborMarketService.Hire(workerCount, Id);
            TypeText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.ProductTypeOutput.ToString();
        }

        public float BuyFromCompany()
        {
            ProducedProduct.Amount--;
            LastSales++;
            TotalSales++;
            Capital += ProducedProduct.Price;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0.##}";;
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


        public void RequestNextStep()
        {
            //Price = ProducedProduct.Price;
            //Count = ProducedProduct.Amount;
            LastCapital = Capital;
            //Capacity = ProductionCapacity;

            if (Capital < 0)
            {
                Debug.Log("Company of type " + ProducedProduct.ProductTypeOutput + " extinct");
                SetReward(-1F);
                Capital = ProducedProduct.ProductTypeOutput == ProductType.Luxury ? 500000 : 100000;
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount, Id);
                ServiceLocator.Instance.LaborMarketService.Hire(2, Id);
                //ServiceLocator.Instance.LaborMarketService.Hire(1);
                ProducedProduct.Price = ProducedProduct.ProductTypeOutput == ProductType.Food ? 1 :
                    ProducedProduct.ProductTypeOutput == ProductType.Intermediate ? 5 : 25;
                ProducedProduct.Amount = 0;
                ResourceStock = 0;
                EndEpisode();
                //Destroy(this);
            }
            
            RequestDecision();
            AddReward(Capital > LastCapital ? 0.1F : -0.1F);
            int workerPayments = SimulatePayWorkers();
            Capital -= workerPayments;
            ServiceLocator.Instance.LaborMarketService.Pay(_workerPayment, Id);
            if (LastSales > 0)
            {
                //Debug.Log("Last sales: " + LastSales);
            }
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0.##}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
            LastSales = 0;
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(Capital);
            sensor.AddObservation(LastSales);
            sensor.AddObservation(ProductionCapacity);
            sensor.AddObservation(WorkerCount);
            sensor.AddObservation(_workerPayment*WorkerCount);
            sensor.AddObservation(ProducedProduct.Price);
            sensor.AddObservation(ProducedProduct.Amount);
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput));
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeOutput));
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int buyResources = actionBuffers.DiscreteActions[0];
            if (buyResources == 1)
            {
                int amount = actionBuffers.DiscreteActions[1] + 1;
                float maxFromCapital = actionBuffers.DiscreteActions[2] + 1;
                BuyRessourceDecision(Capital / maxFromCapital, amount);
            }
            
            int produce = actionBuffers.DiscreteActions[3];
            if (produce == 1)
            {
                Produce();
            }
            
            int hireOrFire = actionBuffers.DiscreteActions[4];
            int count = actionBuffers.DiscreteActions[5];
            if (count == 2)
            {
                throw new Exception();
                
            }
            if (hireOrFire == 1)
            {
                ServiceLocator.Instance.LaborMarketService.Hire(count, Id);
            }
            else if (hireOrFire == 2 && WorkerCount > count)
            {
                ServiceLocator.Instance.LaborMarketService.Fire(count, Id);
            }

            int priceChange = actionBuffers.DiscreteActions[6];
            if (priceChange == 0)
            {
                Debug.Log(priceChange);
            }
            SetPriceDecision(priceChange);
        }

        private void SetPriceDecision(int priceChange)
        {
            float newPrice = priceChange == 0 ? ProducedProduct.Price * 0.99F :
                priceChange == 2 ? ProducedProduct.Price * 1.01F : ProducedProduct.Price;
            ProducedProduct.Price = newPrice;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
        }

        private int SimulatePayWorkers()
        {
            return _workerPayment * WorkerCount;
        }

        private void Produce()
        {
            ProducedProduct.Amount += ProductionCapacity;
            if (ProductionCapacity > 100)
            {
                AddReward(0.01F);
            }
            Capital -= ProductionCapacity * _energyCostPerUnit;
            if (ProducedProduct.ProductTypeInput != ProductType.None)
            {
                ResourceStock -= ProductionCapacity * _ressourceCapacityModifier;
            }
            StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0.##}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }

        private void BuyRessourceDecision(float maxPrice, int amount)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }
            var receipt = ServiceLocator.Instance.ProductMarketService
                .Buy(ProducedProduct.ProductTypeInput, amount, maxPrice);
            if (receipt.CountBought > 0)
            {
                //Debug.Log("Buy");
                AddReward(0.01F);
            }
            Capital -= receipt.AmountPaid;
            ResourceStock += receipt.CountBought;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0.##}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }
    }
}