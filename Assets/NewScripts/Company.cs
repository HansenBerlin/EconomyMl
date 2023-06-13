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
        //private int LastSales;
        private float RoundIncome;
        private float RoundExpenses;
        //private int TotalSales;
        private int WorkerCount => ServiceLocator.Instance.LaborMarketService.CompanyWorkerCount(Id);
        private int ResourceStock;
        //private float LastCapital;
        //public ProductType Type;
        public TextMeshProUGUI TypeText;
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        public TextMeshProUGUI StockText;
        public TextMeshProUGUI MaterialText;
        [FormerlySerializedAs("LastSalesText")] public TextMeshProUGUI RoundIncomeText;
        [FormerlySerializedAs("TotalSalesText")] public TextMeshProUGUI RoundExpensesText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI CapacityText;
        public TextMeshProUGUI ExtinctStatus;

        private int _month;
        
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
        private bool _initDone;

        public void Init(ProductTemplate productTemplate)
        {
            _template = productTemplate;
            ProducedProduct = _template.Product;
            TypeText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.ProductTypeOutput.ToString();
            Reset();
            _initDone = true;
            var ceo = new Worker()
            {
                CompanyId = Id,
                IsCeo = true,
                Money = 300,
                Health = 500
            };
            ServiceLocator.Instance.LaborMarketService.Workers.Add(ceo);
        }

        private void Reset()
        {
            Capital = _template.StartCapital;
            _workerPayment = _template.WorkerSalary;
            _storageCostPerUnit = 0;
            _energyCostPerUnit = 0;
            _workerCapacityModifier = _template.UnitsPerWorker;
            ServiceLocator.Instance.LaborMarketService.Hire(_template.StartWorkerCount, Id);
        }

        public override void OnEpisodeBegin()
        {
            if (_initDone)
            {
                Reset();
            }
        }

        public float BuyFromCompany()
        {
            ProducedProduct.Amount--;
            RoundIncome += ProducedProduct.Price;
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";;
            RoundIncomeText.GetComponent<TextMeshProUGUI>().text = $"{RoundIncome:0}";
            //TotalSalesText.GetComponent<TextMeshProUGUI>().text = TotalSales.ToString();
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
                var maxByRessource = ResourceStock;
                var maxByWorkers = WorkerCount * _workerCapacityModifier;
                capacity = maxByRessource > maxByWorkers ? maxByWorkers : maxByRessource;
            }
            //CapacityText.GetComponent<TextMeshProUGUI>().text = capacity.ToString();
            return capacity;
        }

        public void EndDecade()
        {
            if (Capital > _template.StartCapital)
            {
                SetReward(Capital / _template.StartCapital);
            }
            else
            {
                float negativeReward = _template.StartCapital / Capital * -0.25F;
                SetReward(negativeReward);
            }
            ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount - 1, Id);
            EndEpisode();
        }


        public void RequestNextStep(int month)
        {
            _month = month;
            RoundExpenses = 0;
            RoundIncome = 0;

            if (Capital <= 0)
            {
                AddReward(-0.1F);
                ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount - 1, Id);
                Capital += ServiceLocator.Instance.LaborMarketService.MakeCeoLiable(Id);
            }
            if (Capital > _template.StartCapital * 10)
            {
                AddReward(0.1F);
            }
            if (Capital > _template.StartCapital)
            {
                AddReward(0.01F);
                ServiceLocator.Instance.LaborMarketService.Pay((Capital - _template.StartCapital) / 10, Id, true);
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
            sensor.AddObservation(RoundIncome);
            sensor.AddObservation(ProductionCapacity);
            sensor.AddObservation(RoundExpenses);
            sensor.AddObservation(WorkerCount);
            //sensor.AddObservation(_energyCostPerUnit*ProductionCapacity);
            //sensor.AddObservation(_storageCostPerUnit*ProducedProduct.Amount);
            sensor.AddObservation(ProducedProduct.Price);
            sensor.AddObservation(ProducedProduct.Amount);
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeInput));
            sensor.AddObservation(ServiceLocator.Instance.ProductMarketService.AveragePrice(ProducedProduct.ProductTypeOutput));
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            //if (ProducedProduct.ProductTypeInput == ProductType.None)
            //{
            //    actionMask.SetActionEnabled(0, 0, false);
            //    actionMask.SetActionEnabled(0, 1, false);
            //}
            if (_month >= 13)
            {
                return;
            }
            actionMask.SetActionEnabled(0, 0, false);
            actionMask.SetActionEnabled(1, 0, false);
            actionMask.SetActionEnabled(1, 1, false);
            actionMask.SetActionEnabled(1, 2, false);
            actionMask.SetActionEnabled(4, 0, false);
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int buyResources = actionBuffers.DiscreteActions[0];
            float maxFromCapital = (actionBuffers.DiscreteActions[1] + 1) * 0.25F;
            int hireOrFire = actionBuffers.DiscreteActions[2];
            int count = actionBuffers.DiscreteActions[3];
            int produce = actionBuffers.DiscreteActions[4];
            int priceChange = actionBuffers.DiscreteActions[5];
            //int buyResources = actionBuffers.DiscreteActions[0];

            if (buyResources == 1 && Capital > 0)
            {
                BuyRessourceDecision(WorkerCount * _workerCapacityModifier, Capital * maxFromCapital, float.MaxValue);
            }
            if (hireOrFire == 1)
            {
                ServiceLocator.Instance.LaborMarketService.Hire(count, Id);
            }
            else if (hireOrFire == 2 && WorkerCount > count)
            {
                ServiceLocator.Instance.LaborMarketService.Fire(count, Id);
            }
            if (produce == 1)
            {
                Produce();
            }

            SetPriceDecision(priceChange);
        }

        private void SetPriceDecision(int priceChange)
        {
            float newPrice = priceChange == 0 ? ProducedProduct.Price * 0.999F :
                priceChange == 2 ? ProducedProduct.Price * 1.001F : ProducedProduct.Price;
            newPrice = newPrice < 0.1F ? 0.1F : newPrice;
            ProducedProduct.Price = newPrice;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
        }

        public void EndRound()
        {
            int workerPayments = _workerPayment * WorkerCount;
            RoundExpenses += workerPayments;
            ServiceLocator.Instance.LaborMarketService.Pay(_workerPayment, Id);
            var lastCapital = Capital;
            Capital += RoundIncome;
            Capital -= RoundExpenses;
            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{RoundExpenses:0}";
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";

            AddReward(Capital > lastCapital ? 0.01F : -0.001F);
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

        private void BuyRessourceDecision(int amount, float maxSpending, float maxPricePerPiece)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }
            
            var receipt = ServiceLocator.Instance.ProductMarketService
                .Buy(ProducedProduct.ProductTypeInput, amount, maxSpending, maxPricePerPiece);
            
            RoundExpenses += receipt.AmountPaid;
            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{RoundExpenses:0}";

            ResourceStock += receipt.CountBought;
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
            MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
        }
    }
}