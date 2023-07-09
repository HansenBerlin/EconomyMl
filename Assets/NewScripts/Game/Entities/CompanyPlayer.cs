using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Controls;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Http;
using NewScripts.Interfaces;
using UnityEngine;
using CompanyLedger = NewScripts.DataModelling.CompanyLedger;

namespace NewScripts.Game.Entities
{
    public class CompanyPlayer : MonoBehaviour, ICompany
    {
        //public GameObject canvasInfo;
        public GameObject hourglass;
        public GameObject stageOneBuilding;
        public GameObject stageTwoBuilding;
        public GameObject stageThreeBuilding;
        public GameObject emergencySign;
        private int OpenPositions { get; set; }
        public string Name { get; private set; }
        public int ProductStockFood { get; private set; }
        private int ProductStockLuxury { get; set; }
        public decimal Liquidity { get; set; }
        private int Reputation { get; set; }
        private double Reward { get; set; }
        public int LifetimeMonths { get; private set; } = 1;
        public int WorkerCount => _jobContracts.Count;
        public PlayerType PlayerType { get; } = PlayerType.Human;
        public CompanyDecisionStatus DecisionStatus { get; private set; }
        public decimal AverageWageRate => _jobContracts.Count == 0 ? 100 : _jobContracts.Average(x => x.Wage);

        public Decision LastDecision { get; private set; } = new();
        public int Id => GetInstanceID();
        private readonly List<JobContract> _jobContracts = new();
        private bool _isTraining;
        private bool _writeToDatabase;
        private int _currentActiveIndex = -1;
        //private PlayerDecisionEvent _decisionRequestEvent;
        //public PlayerDecisionEvent DecisionRequestEventProp => _decisionRequestEvent;
        public List<CompanyLedger> Ledger { get; } = new();
        
        private void Awake()
        {
            _isTraining = ServiceLocator.Instance.Settings.IsTraining;
            _writeToDatabase = ServiceLocator.Instance.Settings.WriteToDatabase;
            //_decisionRequestEvent ??= new PlayerDecisionEvent();
            Name = CompanyNames.PickRandomName();
            SetBuilding();
        }

        private void Update() {  
            if (Input.GetMouseButtonDown(0)) {  
                Ray ray = UnityEngine.Camera.main!.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {  
                    if (hit.transform == transform) {  
                        ServiceLocator.Instance.UiUpdateManager.SelectCompanyEvent(this);
                    }  
                }  
            }
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
            if (activeIndex != _currentActiveIndex)
            {
                for (var i = 0; i < buildings.Count; i++)
                {
                    buildings[i].SetActive(i == activeIndex);
                }

                _currentActiveIndex = activeIndex;
            }
        }


        private void AddReward(double reward)
        {
            Reward += reward;
        }

        public Decision AutoDecision()
        {
            var decision = new Decision();
            if (_jobContracts.Count < 40)
            {
                decision.WorkerChange = 40 - _jobContracts.Count;
                decision.Wage = ServiceLocator.Instance.LaborMarket.AveragePayment() * 1.05M;
            }
            else
            {
                decision.WorkerChange = 0;
                decision.Wage = ServiceLocator.Instance.LaborMarket.AveragePayment();
            }

            decision.AdjustWages = true;
            var foodMarket = ServiceLocator.Instance.FoodProductMarket;
            decimal equilibriumPrice = EquilibrieumPriceCalculator.CalculateIntersectionY(
                0, foodMarket.LastMinOfferPrice, foodMarket.LastOffers, foodMarket.LastMaxOfferPrice,
                0, foodMarket.LastMaxBidPrice, foodMarket.LastBids, foodMarket.LastMinBidPrice);
            decision.PriceFood = equilibriumPrice;
            decision.RessourceDistribution = 1;
            return decision;
        }
        
        
        public void StartNextPeriod(Decision decision)
        {
            if (ServiceLocator.Instance.Settings.IsAutoPlay)
            {
                decision = AutoDecision();
            }
            if (DecisionStatus != CompanyDecisionStatus.Requested)
            {
                throw new Exception("DecisionStatus != CompanyDecisionStatus.Requested");
            }
            LastDecision = decision;
            
            var companyData = new CompanyLedger(Id, Name, ServiceLocator.Instance.FlowController.Month, ServiceLocator.Instance.FlowController.Year, LifetimeMonths);
            var averageWage = _jobContracts.Count > 0 ? _jobContracts.Select(x => x.Wage).Average() : 150;
            var workerLedger = new WorkersLedger(_jobContracts.Count, LastDecision.Wage, averageWage);
            var productLedgerFood = new ProductLedger(LastDecision.PriceFood, ProductStockFood);
            var productLedgerLuxury = new ProductLedger(LastDecision.PriceLuxury, ProductStockLuxury);
            var booksLedger = new BookKeepingLedger(Liquidity);
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
            
            if (LastDecision.WorkerChange > 0)
            {
                OpenPositions = LastDecision.WorkerChange;
                workerLedger.OpenPositions = OpenPositions;
                for (int i = 0; i < OpenPositions; i++)
                {
                    var jobBid = new JobBid(this, LastDecision.Wage);
                    ServiceLocator.Instance.LaborMarket.AddJobBid(jobBid);
                }
            }

            if (LastDecision.WorkerChange < 0 && _jobContracts.Count > 0)
            {
                int fireWorkers = LastDecision.WorkerChange * -1;
                for (var i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    if (fireWorkers == 0)
                    {
                        break;
                    }
                    var contract = _jobContracts[i];
                    if (contract.RunsFor > 3)
                    {
                        //workerLedger.WorkersFired++;
                        contract.QuitContract(WorkerFireReason.CompanyDecision);
                        fireWorkers--;
                    }
                }
            }

            companyData.Workers = workerLedger;
            companyData.Food = productLedgerFood;
            companyData.Luxury = productLedgerLuxury;
            companyData.Books = booksLedger;
            companyData.Decision = decisionLedger;
            Ledger.Add(companyData);

            SetBuilding();
            //UpdateCanvasText(false);
            ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Commited);
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            hourglass.SetActive(false);
        }

       public void RequestMonthlyDecision()
       {
           ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Requested);
           ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
           hourglass.SetActive(true);
           hourglass.GetComponent<RotationController>().ActivateAnimation();
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

        private int _lastWorkers;
        
        private void WriteToDatabase(double lastBidProceFood, int lastDemandFood, double lastBidProceLux, int lastDemandLux)
        {
            var ledger = new Http.CompanyLedger
            {
                companyId = Id,
                openPositions = OpenPositions,
                month = ServiceLocator.Instance.FlowController.Month,
                year = ServiceLocator.Instance.FlowController.Year,
                liquidity = (double)Ledger[^1].Books.LiquidityEndCheck,
                profit = (double)(Ledger[^1].Books.LiquidityEndCheck - Ledger[^1].Books.LiquidityStart),
                productionFood = Ledger[^1].Food.Production,
                productionLux = Ledger[^1].Luxury.Production,
                workers = _jobContracts.Count,
                priceFood = (double)Ledger[^2].Food.PriceSet,
                priceLux = (double)Ledger[^2].Luxury.PriceSet,
                wage = (double)Ledger[^1].Workers.AverageWage,
                salesFood = Ledger[^1].Food.Sales,
                salesLux = Ledger[^1].Luxury.Sales,
                lifetime = LifetimeMonths,
                sessionId = ServiceLocator.Instance.SessionId,
                isTraining = _isTraining,
                marketBidPriceFood = lastBidProceFood,
                marketDemandFood = lastDemandFood,
                marketBidPriceLux = lastBidProceLux,
                marketDemandLux = lastDemandLux,
                ressourceAllocation = Ledger[^2].Decision.ResourceDistribution
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
                    //contract.QuitContract(WorkerFireReason.LackOfFunds);
                }
            }

            //PaySocialFare();
            
            LifetimeMonths++;
            
            
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
            decimal taxPaid = 0;
            if (LifetimeMonths > 3 && profit > 0)
            {
                taxPaid = ServiceLocator.Instance.Government.PayCompanyTaxes(profit);
                Liquidity -= taxPaid;
            }
            
            Ledger[^1].Books.TaxPayments = taxPaid;
            
            
            SetBuilding();
            _lastWorkers = _jobContracts.Count;

            
            


            
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
        }

        public void AddRewards(int year, double lastBidProceFood, int lastDemandFood, double lastBidProceLux, int lastDemandLux)
        {
            Ledger[^1].Books.LiquidityEndCheck = Liquidity;
            if (_writeToDatabase)
            {
                WriteToDatabase(lastBidProceFood, lastDemandFood, lastBidProceLux, lastDemandLux);
            }
            Ledger[^1].Reputation = Reputation;
            ServiceLocator.Instance.HouseholdAggregator.Add(Ledger[^1]);
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            ServiceLocator.Instance.FlowController.CommitCompanyDecision(Id, DecisionStatus = CompanyDecisionStatus.Pending);
        }

        private void PaySocialFare()
        {
            decimal baseSafety = 100000 / (decimal) ServiceLocator.Instance.Companys.Count / 4;
            var liquidity = Liquidity;
            if (Liquidity > baseSafety)
            {
                //AddReward((float)Liquidity - (float)baseSafety);
                decimal companyReserve = (Liquidity - baseSafety) * 0.10M;
                var unemployed =
                    ServiceLocator.Instance.LaborMarket.Workers.Where(
                        x => x.HasJob == false && x.Money < 100).ToList();

                if (unemployed.Count > 0)
                {
                    decimal societyShare = (Liquidity - baseSafety - companyReserve) / unemployed.Count;
                    foreach (var worker in unemployed)
                    {
                        worker.PaySocialWelfare(societyShare);
                        Ledger[^1].Books.TaxPayments += societyShare;
                    }
                    Liquidity = companyReserve + baseSafety;
                }


                var companys = ServiceLocator.Instance.Companys.Select(x => x.Liquidity).Sum();
                var workers = ServiceLocator.Instance.LaborMarket.Workers.Select(x => x.Money).Sum();
                if (companys + workers < 99999.9M || companys + workers > 100000.1M)
                {
                    Debug.LogError("Society is bankrupt " + Liquidity + " " + liquidity);
                }

            }
            //AddReward((float)societyShare);

            //var unemployed =
            //    ServiceLocator.Instance.LaborMarket.Workers.Where(
            //        x => x.HasJob == false && x.Money < 100).ToList();
            //double societyShare = unemployed.Count > 0 ? (Liquidity - companyReserve) / unemployed.Count : 0;
//
            //foreach (var worker in unemployed)
            //{
            //    worker.Pay(societyShare);
            //}
//
            //Liquidity = societyShare > 0 ? companyReserve : Liquidity;
        }

        public void AddContract(JobContract contract)
        {
            _jobContracts.Add(contract);
            Reputation++;
            Ledger[^1].Workers.Hired++;
            AddReward(1F);
        }
        
        public void RemoveContract(JobContract contract, WorkerFireReason reason)
        {
            _jobContracts.Remove(contract);
            Reputation--;
            AddReward(-1.1F);
            if (reason == WorkerFireReason.CompanyDecision)
            {
                Ledger[^1].Workers.FiredByDecision++;
            }
            else if (reason == WorkerFireReason.LackOfFunds)
            {
                Ledger[^1].Workers.FiredByLackOfFunds++;
            }
            else
            {
                Ledger[^1].Workers.Quit++;
            }
        }
        
        public void FullfillBid(ProductType product, int count, decimal price)
        {
            if(product == ProductType.Luxury)
            {
                Ledger[^1].Luxury.Sales += count;
                ProductStockLuxury -= count;
            }
            else if(product == ProductType.Food)
            {
                Ledger[^1].Food.Sales += count;
                ProductStockFood -= count;
            }
            
            Ledger[^1].Books.Income += count * price;
            Liquidity += count * price;
            AddReward((float)count / 10000);
        }
    }
}
