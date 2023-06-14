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
    public class CompanyExp : Agent
    {
        /*private int Id => GetInstanceID();

        private float _capital;
        //public float Count;
        //public float Price;
        //public float Capacity;
        private float _roundIncome;

        private float _roundExpenses;
        //private int TotalSales;
        private int WorkerCount => ServiceLocator.Instance.LaborMarketService.CompanyWorkerCount(Id);
        private int _resourceStock;
        //private float LastCapital;
        //public ProductType Type;
        public TextMeshProUGUI TypeText;
        public TextMeshProUGUI CapitalText;
        public TextMeshProUGUI WorkersText;
        public TextMeshProUGUI StockText;
        public TextMeshProUGUI MaterialText;
        public TextMeshProUGUI RoundIncomeText;
        public TextMeshProUGUI RoundExpensesText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI CapacityText;
        public TextMeshProUGUI ExtinctStatus;
        private int companyAge = 0;


        private int ProductionCapacity => CalculateProductionCapacity();
        public Product ProducedProduct { get; private set; }

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
            //TypeText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.ProductTypeOutput.ToString();
            var ceo = new Worker
            {
                CompanyId = Id,
                IsCeo = true,
                Money = 2000,
                Health = 500
            };
            ServiceLocator.Instance.LaborMarketService.Workers.Add(ceo);
            //Reset();
            _initDone = true;
        }

        private void Reset()
        {
            _capital = _template.StartCapital;
            _workerPayment = _template.WorkerSalary;
            _storageCostPerUnit = 0;
            _energyCostPerUnit = 0;
            _workerCapacityModifier = _template.UnitsPerWorker;
            ProducedProduct = _template.Product;
            ServiceLocator.Instance.LaborMarketService.Hire(_template.StartWorkerCount, Id);
        }

        public override void OnEpisodeBegin()
        {
            companyAge = 0;
            if (_initDone)
            {
                Reset();
            }
        }

        public float BuyFromCompany(int amount)
        {
            ProducedProduct.Amount -= amount;
            if (ProducedProduct.Amount < 0)
            {
                throw new Exception("Less than zero.");
            }
            _roundIncome += ProducedProduct.Price * amount;
            RoundIncomeText.GetComponent<TextMeshProUGUI>().text = $"{_roundIncome:0}";
            return ProducedProduct.Price * amount;
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
                var maxByRessource = _resourceStock;
                var maxByWorkers = WorkerCount * _workerCapacityModifier;
                capacity = maxByRessource > maxByWorkers ? maxByWorkers : maxByRessource;
            }
            //CapacityText.GetComponent<TextMeshProUGUI>().text = capacity.ToString();
            return capacity;
        }

        public void EndCompanyEpisode()
        {
            if (_capital > _template.StartCapital)
            {
                SetReward(_capital / _template.StartCapital);
            }
            else
            {
                float negativeReward = _template.StartCapital / _capital * -0.25F;
                SetReward(negativeReward);
            }
            ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount, Id);
            EndEpisode();
        }


        public void RequestNextStep()
        {
            companyAge++;
            //RoundExpenses = 0;
            //RoundIncome = 0;
            //RoundIncomeText.GetComponent<TextMeshProUGUI>().text = $"{RoundIncome:0}";
            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{_roundExpenses:0}";

            if (_capital > _template.StartCapital)
            {
                AddReward(_capital / _template.StartCapital * 0.001F);
            }
            else if(_capital * -0.0001F != 0)
            {
                float negativeReward = _template.StartCapital / _capital * -0.0001F;
                AddReward(negativeReward);
            }

            if (_capital <= 0)
            {
                //ServiceLocator.Instance.LaborMarketService.Fire(WorkerCount, Id);
                _capital += ServiceLocator.Instance.LaborMarketService.MakeCeoLiable(Id);
            }

            RequestDecision();
            CapacityText.GetComponent<TextMeshProUGUI>().text = ProductionCapacity.ToString();
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
            //StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{_capital:0}";
            WorkersText.GetComponent<TextMeshProUGUI>().text = WorkerCount.ToString();
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_capital);
            sensor.AddObservation(_roundIncome);
            sensor.AddObservation(ProductionCapacity);
            sensor.AddObservation(_roundExpenses);
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
            actionMask.SetActionEnabled(4, 0, false);
            if (ServiceLocator.Instance.FlowController.Day != 30)
            {
                actionMask.SetActionEnabled(2, 1, false);
                actionMask.SetActionEnabled(2, 2, false);
            }
            
            if (companyAge > 365)
            {
                return;
            }
            
            actionMask.SetActionEnabled(2, 2, false);
            actionMask.SetActionEnabled(0, 0, false);
            actionMask.SetActionEnabled(1, 0, false);
            actionMask.SetActionEnabled(1, 1, false);
            actionMask.SetActionEnabled(1, 2, false);
        }

        public void StartMonth()
        {
            // Lohnsatz anpassen
            // Verkaufspreis anpassen
            // Mitarbeiterzahl anpassn (offene Stellen)
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int buyResources = actionBuffers.DiscreteActions[0];
            float maxFromCapital = (actionBuffers.DiscreteActions[1] + 1) * 0.25F;
            
            int produce = actionBuffers.DiscreteActions[4];
            int priceChange = actionBuffers.DiscreteActions[5];
            //int buyResources = actionBuffers.DiscreteActions[0];

            if (buyResources == 1 && _capital > 0)
            {
                BuyRessourceDecision(WorkerCount * _workerCapacityModifier, _capital * maxFromCapital, float.MaxValue);
            }

            if (ServiceLocator.Instance.FlowController.Day == 30)
            {
                int hireOrFire = actionBuffers.DiscreteActions[2];
                
                float count = (actionBuffers.DiscreteActions[3] + 1) * 0.25F * WorkerCount;
                if (hireOrFire == 1)
                {
                    ServiceLocator.Instance.LaborMarketService.Hire((int)Math.Ceiling(count), Id);
                }
                else if (hireOrFire == 2)
                {
                    ServiceLocator.Instance.LaborMarketService.Fire((int)Math.Ceiling(count), Id);
                }
            }
            if (produce == 1)
            {
                Produce();
            }
            else
            {
                Debug.Log("dfdf");
            }

            SetPriceDecision(priceChange);
        }

        private void SetPriceDecision(int priceChange)
        {
            float newPrice = priceChange == 0 ? ProducedProduct.Price * 0.995F :
                priceChange == 2 ? ProducedProduct.Price * 1.005F : ProducedProduct.Price;
            newPrice = newPrice < 0.1F ? 0.1F : newPrice;
            ProducedProduct.Price = newPrice;
            PriceText.GetComponent<TextMeshProUGUI>().text = $"{ProducedProduct.Price:0.##}";
        }

        public void PayWorkers()
        {
            int workerPayments = _workerPayment * (WorkerCount - 1);
            ServiceLocator.Instance.LaborMarketService.Pay(WorkerCount, Id);
            
            float gap = _capital - _roundExpenses - workerPayments;
            if (gap < 0)
            {
                AddReward(-0.01F);
                int unpaidWorkers = (int)Math.Ceiling(gap * -1 / WorkerCount);
                ServiceLocator.Instance.LaborMarketService.Fire(unpaidWorkers, Id);
                //workerPayments = _workerPayment * (WorkerCount - 1);
            }
            _roundExpenses += workerPayments;
            
            //ServiceLocator.Instance.LaborMarketService.Pay(_workerPayment, Id);
            if (_capital - _roundExpenses > _template.StartCapital)
            {
                AddReward(0.02F);
                float ceoShare = (_capital - _roundExpenses - _template.StartCapital) / 10;
                ServiceLocator.Instance.LaborMarketService.Pay(ceoShare, Id, true);
                _roundExpenses += ceoShare;
            }

            if (_capital <= 0 && WorkerCount == 1)
            {
                EndCompanyEpisode();
            }

            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{_roundExpenses:0}";
        }

        public void EndRound()
        {
            var lastCapital = _capital;
            _capital += _roundIncome;
            _capital -= _roundExpenses;
            CapitalText.GetComponent<TextMeshProUGUI>().text = $"{_capital:0}";
            RoundIncomeText.GetComponent<TextMeshProUGUI>().text = $"{_roundIncome:0}";
            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{_roundExpenses:0}";
            _roundIncome = 0;
            _roundExpenses = 0;

            AddReward(_capital > lastCapital ? 0.001F : -0.00001F);
        }

        private void Produce()
        {
            //MaterialText.GetComponent<TextMeshProUGUI>().text = ResourceStock.ToString();
            if (ProductionCapacity > _template.UnitsPerWorker * 10)
            {
                AddReward(0.001F);
            }
            
            ProducedProduct.Amount += ProductionCapacity;
            //Capital -= ProductionCapacity * _energyCostPerUnit;
            if (ProducedProduct.ProductTypeInput != ProductType.None)
            {
                _resourceStock -= ProductionCapacity;
            }
            //StockText.GetComponent<TextMeshProUGUI>().text = ProducedProduct.Amount.ToString();
            //CapitalText.GetComponent<TextMeshProUGUI>().text = $"{Capital:0}";
        }

        private void BuyRessourceDecision(int amount, float maxSpending, float maxPricePerPiece)
        {
            if (ProducedProduct.ProductTypeInput == ProductType.None)
            {
                return;
            }

            if (_capital == 0 && WorkerCount == 1 && _resourceStock == 0)
            {
                _resourceStock = 2;
                return;
            }
            
            var receipt = ServiceLocator.Instance.ProductMarketService
                .Buy(ProducedProduct.ProductTypeInput, amount, maxSpending, maxPricePerPiece);
            
            _roundExpenses += receipt.AmountPaid;
            RoundExpensesText.GetComponent<TextMeshProUGUI>().text = $"{_roundExpenses:0}";

            _resourceStock += receipt.CountBought;
            MaterialText.GetComponent<TextMeshProUGUI>().text = _resourceStock.ToString();
        }*/
    }
}