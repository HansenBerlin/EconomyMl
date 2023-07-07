using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Http;
using NewScripts.Interfaces;
using NewScripts.Training;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using CompanyLedger = NewScripts.DataModelling.CompanyLedger;

namespace NewScripts.Game.Entities
{
    public class Company : Agent, ICompany
    {
        //public GameObject canvasInfo;
        public GameObject stageZeroBuilding;
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        //public PlayerDecisionEvent DecisionRequestEventProp { get; }
        public CompanyDecisionStatus DecisionStatus { get; private set; }


        public string Name { get; private set; }
        public PlayerType PlayerType { get; } = PlayerType.Ai;
        private int OpenPositions { get; set; }
        public int ProductStockFood { get; private set; }
        private int ProductStockLuxury { get; set; }
        public decimal Liquidity { get; set; }
        //private int Reputation { get; set; }
        public int LifetimeMonths { get; private set; } = 1;
        public int Id => GetInstanceID();
        private readonly List<JobContract> _jobContracts = new();
        private bool _isTraining;
        private bool _writeToDatabase;
        public List<CompanyLedger> Ledger { get; } = new();
        public int WorkerCount => _jobContracts.Count;
        public decimal AverageWageRate => _jobContracts.Count == 0 ? 150 : _jobContracts.Average(x => x.Wage);
        public Decision LastDecision { get; private set; } = new();
        private ReputationAggregator _reputationAggregator;

        
        private void Start()
        {
            _isTraining = ServiceLocator.Instance.Settings.IsTraining;
            _writeToDatabase = ServiceLocator.Instance.Settings.WriteToDatabase;
            //SetupAgent();
            Name = CompanyNames.PickRandomName();
            _reputationAggregator = ServiceLocator.Instance.ReputationAggregatorFactory.Create();
        }

        private void Update() {  
            if (Input.GetMouseButtonDown(0)) {  
                Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {  
                    if (hit.transform == transform) {  
                        ServiceLocator.Instance.UiUpdateManager.SelectCompanyEvent(this);
                    }  
                }  
            } 
        }

        private void SetupAgent()
        {
            LifetimeMonths = 0;
            ProductStockFood /= 2;
            ProductStockLuxury /= 2;
            _jobRampup = 0;
            _foodPriceRampup = 0.6F;
            _luxPriceRampup = 6;
            _wageRampup = 70;
            _reputationAggregator = ServiceLocator.Instance.ReputationAggregatorFactory.Create();
            SetBuilding();
        }

        private void SetBuilding()
        {
            var buildings = new List<GameObject>
            {
                stageOneBuilding, stageTwoBuilding, stageThreeBuilding
            };
            double companycount = ServiceLocator.Instance.Companys.Count;
            double personCount = ServiceLocator.Instance.LaborMarket.Workers.Count;
            double ratio = personCount / companycount;
            int activeIndex = _jobContracts.Count < ratio * 2 ? 0 : _jobContracts.Count > ratio * 5 ? 2 : 1;
            for (var i = 0; i < buildings.Count; i++)
            {
                buildings[i].SetActive(i == activeIndex);
            }
        }

        public void RequestMonthlyDecision()
        {
            ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Requested);
            RequestDecision();
        }

        public void StartNextPeriod(Decision decision)
        {
            //throw new NotImplementedException();
        }

        public override void OnEpisodeBegin()
        {
            SetupAgent();
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            if (Ledger.Count > 0)
            {
                sensor.AddObservation((float)Ledger.Last().Books.Income);
                sensor.AddObservation((float)Ledger.Last().Books.WagePayments);
                sensor.AddObservation((float)Ledger.Last().Books.LiquidityStart);
                sensor.AddObservation((float)Ledger.Last().Books.LiquidityEndCheck);
                sensor.AddObservation((float)Ledger.Last().Month);
                sensor.AddObservation((float)Ledger.Last().Reputation);
                sensor.AddObservation((float)Ledger.Last().Lifetime);
                sensor.AddObservation((float) Ledger.Last().Decision.SetWorkerWage);
                sensor.AddObservation((float) Ledger.Last().Decision.OpenPositions);
                sensor.AddObservation((float) Ledger.Last().Decision.SetFoodPrice);
                sensor.AddObservation((float) Ledger.Last().Decision.SetLuxuryPrice);
                sensor.AddObservation((float) Ledger.Last().Workers.StartCount);
                sensor.AddObservation((float) Ledger.Last().Workers.EndCount);
                sensor.AddObservation((float) Ledger.Last().Workers.Hired);
                sensor.AddObservation((float) Ledger.Last().Workers.FiredByDecision);
                sensor.AddObservation((float) Ledger.Last().Workers.FiredByLackOfFunds);
                sensor.AddObservation((float) Ledger.Last().Food.Production);
                sensor.AddObservation((float) Ledger.Last().Food.Sales);
                sensor.AddObservation((float) Ledger.Last().Food.Destroyed);
                sensor.AddObservation((float) Ledger.Last().Food.StockStart);
                sensor.AddObservation((float) Ledger.Last().Food.StockEndCheck);
                sensor.AddObservation((float) Ledger.Last().Luxury.Production);
                sensor.AddObservation((float) Ledger.Last().Luxury.Sales);
                sensor.AddObservation((float) Ledger.Last().Luxury.Destroyed);
                sensor.AddObservation((float) Ledger.Last().Luxury.StockStart);
                sensor.AddObservation((float) Ledger.Last().Luxury.StockEndCheck);
                sensor.AddObservation((float)ServiceLocator.Instance.LaborMarket.AveragePayment());
                sensor.AddObservation((float)ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear());
                sensor.AddObservation((float)ServiceLocator.Instance.LuxuryProductMarket.AveragePriceInLastYear());
                sensor.AddObservation((float)ServiceLocator.Instance.FoodProductMarket.DemandForProduct);
                sensor.AddObservation((float)ServiceLocator.Instance.LuxuryProductMarket.DemandForProduct);
                sensor.AddObservation((float)ServiceLocator.Instance.LaborMarket.DemandForWorkforce);
            }
        }

        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (_jobContracts.Count == 0)
            {
                //actionMask.SetActionEnabled(0, 0, false);
                //actionMask.SetActionEnabled(0, 2, false);
            }
        }

        private float _jobRampup;
        private float _foodPriceRampup = 0.6F;
        private float _luxPriceRampup = 6;
        private float _wageRampup = 70;
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            //UpdateCanvasText(false);
            if (DecisionStatus != CompanyDecisionStatus.Requested)
            {
                throw new Exception("DecisionStatus != CompanyDecisionStatus.Requested");
            }

            if(LifetimeMonths == 12)
            {
                _jobRampup = Math.Abs(_jobRampup - 1) > 0.01F ? _jobRampup + 0.1F : 1;
                _wageRampup = _wageRampup > 0 ? _wageRampup - 7 : 0;
                _foodPriceRampup = _foodPriceRampup > 0 ? _foodPriceRampup - 0.06F : 0;
                _luxPriceRampup = _luxPriceRampup > 0 ? _luxPriceRampup - 0.6F : 0;
            }

            LastDecision = new Decision();
            int workerDecision = actionBuffers.DiscreteActions[0];
            LastDecision.AdjustWages = actionBuffers.DiscreteActions[1] == 1;
            LastDecision.Wage = (decimal)ValueMapper.MapValue(actionBuffers.ContinuousActions[0], 10 + _wageRampup, 500 - 4 * _wageRampup);
            LastDecision.PriceFood = (decimal)ValueMapper.MapValue(actionBuffers.ContinuousActions[1], 0.1F + _foodPriceRampup, 5F - _foodPriceRampup * 5);
            LastDecision.PriceLuxury = (decimal)ValueMapper.MapValue(actionBuffers.ContinuousActions[2], 1F + _luxPriceRampup, 100F - _luxPriceRampup * 10);
            LastDecision.RessourceDistribution = ValueMapper.MapValue(actionBuffers.ContinuousActions[3], 0.1F, 1F);

            if (LastDecision.Wage > 1000)
            {
                Debug.Log("Wage too high");
            }
            var companyData = new CompanyLedger(Id, Name, ServiceLocator.Instance.FlowController.Month, 
                ServiceLocator.Instance.FlowController.Year, LifetimeMonths);
            var averageLocalWage = _jobContracts.Count > 0 ? _jobContracts.Select(x => x.Wage).Average() : 150;
            var workerLedger = new WorkersLedger(_jobContracts.Count, LastDecision.Wage, averageLocalWage);
            var foodProductLedger = new ProductLedger(LastDecision.PriceFood, ProductStockFood);
            var luxuryProductLedger = new ProductLedger(LastDecision.PriceLuxury, ProductStockLuxury);
            var booksLedger = new BookKeepingLedger(Liquidity);
            int fireWorkers = (int)ValueMapper.MapValue(actionBuffers.ContinuousActions[4], 1, _jobContracts.Count);
            
            if(workerDecision == 1)
            {
                OpenPositions = (int)ValueMapper.MapValue(actionBuffers.ContinuousActions[5], 1, 1000 * _jobRampup);
                LastDecision.WorkerChange = OpenPositions;
            }
            else if(workerDecision == 2 && _jobContracts.Count > 0)
            {
                LastDecision.WorkerChange = fireWorkers * -1;
            }
            else
            {
                LastDecision.WorkerChange = 0;
            }

            var decisionLedger = new DecisionLedger(LastDecision);

            if (LastDecision.AdjustWages)
            {
                foreach (var contract in _jobContracts)
                {
                    if (contract.Wage < LastDecision.Wage)
                    {
                        contract.Wage = LastDecision.Wage;
                    }
                }
            }

            if (workerDecision == 1)
            {
                workerLedger.OpenPositions = OpenPositions;
                for (int i = 0; i < OpenPositions; i++)
                {
                    var jobBid = new JobBid(this, LastDecision.Wage);
                    ServiceLocator.Instance.LaborMarket.AddJobBid(jobBid);
                }
            }

            if (workerDecision == 2 && _jobContracts.Count > 0)
            {
                for (var i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    if (fireWorkers == 0)
                    {
                        break;
                    }
                    var contract = _jobContracts[i];
                    if (contract.RunsFor > 3)
                    {
                        contract.QuitContract(WorkerFireReason.CompanyDecision);
                        fireWorkers--;

                    }
                }
            }

            companyData.Workers = workerLedger;
            companyData.Food = foodProductLedger;
            companyData.Luxury = luxuryProductLedger;
            companyData.Books = booksLedger;
            companyData.Decision = decisionLedger;
            Ledger.Add(companyData);

            SetBuilding();
            //UpdateCanvasText(false);
            ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Commited);
            //UpdateCanvasText(false);

        }

        

        public void Produce()
        {
            int foodProduction = (int)Math.Floor(_jobContracts.Count *
                                                 ServiceLocator.Instance.Settings.OutputMultiplier(ProductType.Food) *
                                                 LastDecision.RessourceDistribution);
            ProductStockFood += foodProduction;
            Ledger[^1].Food.Production = foodProduction;
            if (ProductStockFood > 0)
            {
                var offer = new ProductOffer(ProductType.Food, this, LastDecision.PriceFood, ProductStockFood);
                ServiceLocator.Instance.FoodProductMarket.AddOffer(offer);
            }
            
            int luxuryProduction = (int)Math.Floor(_jobContracts.Count *
                                                   ServiceLocator.Instance.Settings.OutputMultiplier(ProductType.Luxury) *
                                                   (1 - LastDecision.RessourceDistribution));
            ProductStockLuxury += luxuryProduction;
            Ledger[^1].Luxury.Production = luxuryProduction;
            if (ProductStockLuxury > 0)
            {
                var offer = new ProductOffer(ProductType.Luxury, this, LastDecision.PriceLuxury, ProductStockLuxury);
                ServiceLocator.Instance.LuxuryProductMarket.AddOffer(offer);
            }
            
            //UpdateCanvasText(false);
        }

        private int lastWorkers;

        private void WriteToDatabase()
        {
            var ledger = new Http.CompanyLedger
            {
                companyId = Id,
                openPositions = OpenPositions,
                month = ServiceLocator.Instance.FlowController.Month,
                year = ServiceLocator.Instance.FlowController.Year,
                liquidity = (double)Liquidity,
                realWage = _jobContracts.Count > 0 ? (double)_jobContracts.Average(x => x.Wage) : 0,
                workers = _jobContracts.Count,
                //price = (double)ProductPriceFood,
                //wage = (double)OfferedWageRate,
                //sales = _salesInMonth,
                stock = ProductStockFood,
                lifetime = LifetimeMonths,
                sessionId = ServiceLocator.Instance.SessionId,
                emergencyRounds = 0,
                isStartup = false,
                isTraining = _isTraining
            };
            StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
        }
        
        public void EndMonth()
        {
            for (var i = _jobContracts.Count - 1; i >= 0; i--)
            {
                var contract = _jobContracts[i];
                if (contract.Wage < Liquidity)
                {
                    contract.PayWorker();
                    Ledger[^1].Workers.ReducedPaidCount++;
                    Ledger[^1].Books.WagePayments += contract.Wage;
                }
                else if (contract.Wage / 2 < Liquidity && contract.ShortWorkForMonths <= 3)
                {
                    contract.PayReducedWage();
                    Ledger[^1].Workers.ReducedPaidCount++;
                    Ledger[^1].Books.WagePayments += contract.Wage / 2;
                }
                else
                {
                    if (LifetimeMonths > 24)
                    {
                        contract.QuitContract(WorkerFireReason.LackOfFunds);
                    }
                }
            }

            //PaySocialFare();
            
            LifetimeMonths++;
            
            
            //UpdateCanvasText(false);
            //AddReward(Reputation*0.01F);
            //AddReward((float)Liquidity*0.01F);
            //int availableSpace = _jobContracts.Count * 400;
            int destroy = (int)(ProductStockFood * 0.1M);
            Ledger[^1].Food.Destroyed = destroy;
            ProductStockFood -= destroy;
            ProductStockLuxury -= (int)(ProductStockLuxury * 0.1M);
            Ledger[^1].Food.StockEndCheck = ProductStockFood;
            Ledger[^1].Luxury.StockEndCheck = ProductStockLuxury;
            Ledger[^1].Books.LiquidityEndCheck = Liquidity;
            Ledger[^1].Workers.EndCount = _jobContracts.Count;

            var lastPeriodData = Ledger[^1];
            decimal profit = lastPeriodData.Books.LiquidityEndCheck - lastPeriodData.Books.LiquidityStart;
            if (ServiceLocator.Instance.FlowController.Year > 1 && profit > 0)
            {
                Liquidity -= ServiceLocator.Instance.Government.PayTaxes(profit);
            }
            _reputationAggregator.AddValuesToNormalizers((double)profit, lastPeriodData.Lifetime, _jobContracts, lastPeriodData.Food.Sales, lastPeriodData.Luxury.Sales);

            
            ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Pending);
            
            SetBuilding();
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
            lastWorkers = _jobContracts.Count;
        }

        public void AddRewards(int year)
        {
            _reputationAggregator.AddLifetimeChange();
            _reputationAggregator.AddProfitChange();
            _reputationAggregator.AddMarketShareChange();
            _reputationAggregator.AddWorkerContractRuntimeChange();
            float reputation = _reputationAggregator.Reputation;
            Ledger[^1].Reputation = reputation;
            AddReward(reputation);
            ServiceLocator.Instance.HouseholdAggregator.Add(Ledger[^1]);
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            if (reputation <= -0.75 && year > 2)
            {
                SetReward(-100);
                EndEpisode();
            }
            else if (reputation >= 0.75 && year > 2)
            {
                SetReward(100);
                EndEpisode();
            }
        }

        public void AddContract(JobContract contract)
        {
            _jobContracts.Add(contract);
            Ledger[^1].Workers.Hired++;
            //AddReward(0.1F);
        }
        
        public void RemoveContract(JobContract contract, WorkerFireReason reason)
        {
            _jobContracts.Remove(contract);
            if (reason == WorkerFireReason.CompanyDecision)
            {
                Ledger[^1].Workers.FiredByDecision++;
                //AddReward(-0.05F);
            }
            else if (reason == WorkerFireReason.LackOfFunds)
            {
                Ledger[^1].Workers.FiredByLackOfFunds++;
                //AddReward(-0.1F);
            }
            else
            {
                Ledger[^1].Workers.Quit++;
                //AddReward(-0.01F);
            }
        }
        
        public void FullfillBid(ProductType product, int count, decimal price)
        {
            if(product == ProductType.Luxury)
            {
                Ledger[^1].Luxury.Sales += count;
                ProductStockLuxury -= count;
                //Reputation++;
            }
            else if(product == ProductType.Food)
            {
                Ledger[^1].Food.Sales += count;
                ProductStockFood -= count;
            }
            
            Ledger[^1].Books.Income += count * price;
            //ProductStockFood -= count;
            Liquidity += count * price;
            //AddReward(0.01F);
        }
    }
}
