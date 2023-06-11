using System;
using System.Collections.Generic;
using Agents;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts
{
    public class Company : Agent
    {
        public int Id;
        public float Capital;
        public float Count;
        public float Price;
        public float Capacity;
        public int LastSales;
        public int TotalSales;
        public int WorkerCount;
        public int ResourceStock = 5000;
        public float LastCapital;
        public ProductType Type;
        public GameObject LaborMarketGameObject;
        public GameObject ProductMarketGameObject;
        public int ProductionCapacity => CalculateProductionCapacity();
        public Product ProducedProduct { get; set; }

        private ProductMarket _productMarket;
        private LaborMarket _laborMarket;
        private int _workerPayment;
        private int _ressourceCapacityModifier;
        private int _workerCapacityModifier;
        private float _energyCostPerUnit;

        public void Init(Product produced)
        {
            Id = GetInstanceID();
            ProducedProduct = produced;
            Type = ProducedProduct.ProductTypeOutput;
            ProductMarketGameObject = GameObject.Find("ProductMarket");
            LaborMarketGameObject = GameObject.Find("LaborMarket");
            _productMarket = ProductMarketGameObject.GetComponent<ProductMarket>();
            _laborMarket = LaborMarketGameObject.GetComponent<LaborMarket>();
            _productMarket.payCompanyEvent.AddListener((x) =>
            {
                if (int.Parse(x) == Id)
                {
                    ProducedProduct.Amount--;
                    LastSales++;
                    Capital += ProducedProduct.Price;
                    Debug.Log("Sold!");
                }
            });
            _productMarket.Companys.Add(this);
            _workerPayment = produced.ProductTypeOutput == ProductType.Food ? 35 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 35 : 60;
            WorkerCount = produced.ProductTypeOutput == ProductType.Food ? 200 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 500 : 200;
            _energyCostPerUnit = produced.ProductTypeOutput == ProductType.Food ? 0.2F :
                produced.ProductTypeOutput == ProductType.Intermediate ? 1 : 5;
            _workerCapacityModifier = produced.ProductTypeOutput == ProductType.Food ? 50 :
                produced.ProductTypeOutput == ProductType.Intermediate ? 10 : 5;
            _ressourceCapacityModifier = produced.ProductTypeOutput == ProductType.Luxury ? 5 : 1;
        }

        public void MakeSale()
        {
            ProducedProduct.Amount--;
            LastSales++;
            TotalSales++;
            Capital += ProducedProduct.Price;
            //Debug.Log("Sold!");
        }

        private int CalculateProductionCapacity()
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return WorkerCount * _workerCapacityModifier;
            }

            return (int) Math.Round((double) (ResourceStock / _ressourceCapacityModifier * WorkerCount * _workerCapacityModifier), 0);
        }


        public void RequestNextStep()
        {
            Price = ProducedProduct.Price;
            Count = ProducedProduct.Amount;
            LastCapital = Capital;
            Capacity = ProductionCapacity;
            if (Capital < 0)
            {
                Debug.Log("Company of type " + ProducedProduct.ProductTypeOutput + " extinct");
                _productMarket.RemoveOffer(Id);
                SetReward(-1F);
                Capital = 100000;
                WorkerCount -= _laborMarket.Fire(WorkerCount);
                _laborMarket.Hire(10);
                EndEpisode();
                //Destroy(this);
            }
            
            RequestDecision();
            AddReward(Capital > LastCapital ? 0.1F : -0.1F);
            int workerPayments = SimulatePayWorkers();
            _laborMarket.Pay(workerPayments);
            if (LastSales > 0)
            {
                //Debug.Log("Last sales: " + LastSales);
            }
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
            sensor.AddObservation(_productMarket.AveragePrice(ProducedProduct.ProductTypeInput));
            sensor.AddObservation(_productMarket.AveragePrice(ProducedProduct.ProductTypeOutput));
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int buyResources = actionBuffers.DiscreteActions[0];
            if (buyResources == 1)
            {
                int amount = actionBuffers.DiscreteActions[1];
                float maxPrice = actionBuffers.DiscreteActions[2];
                BuyRessourceDecision(maxPrice, amount);
            }
            
            int produce = actionBuffers.DiscreteActions[3];
            if (produce == 1)
            {
                Produce();
            }
            
            int hireOrFire = actionBuffers.DiscreteActions[4];
            int count = actionBuffers.DiscreteActions[5];
            if (hireOrFire == 1)
            {
                _laborMarket.Hire(count);
                WorkerCount += count;
            }
            else if (hireOrFire == 2 && WorkerCount > count)
            {
                WorkerCount -= _laborMarket.Fire(count);
            }

            int priceChange = actionBuffers.DiscreteActions[6];
            SetPriceDecision(priceChange);
        }

        private void SetPriceDecision(int priceChange)
        {
            float newPrice = priceChange == 0 ? ProducedProduct.Price * 0.95F :
                priceChange == 2 ? ProducedProduct.Price * 1.05F : ProducedProduct.Price;
            ProducedProduct.Price = newPrice;
            ProductOffer offer = new()
            {
                OfferedBy = this,
                Product = ProducedProduct
            };
            _productMarket.AddOffer(offer);
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
                ResourceStock -= ProductionCapacity / _ressourceCapacityModifier;
            }
        }

        private void BuyRessourceDecision(float maxPrice, int amount)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }
            var receipt = _productMarket.Buy(ProducedProduct.ProductTypeInput, amount, maxPrice);
            if (receipt.CountBought > 0)
            {
                Debug.Log("Buy");
                AddReward(0.01F);
            }
            Capital -= receipt.AmountPaid;
            ResourceStock += receipt.CountBought;
        }
    }
}