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
        
        
        public void StartNextPeriod(Decision decision)
        {
            
            if (DecisionStatus != CompanyDecisionStatus.Requested)
            {
                throw new Exception("DecisionStatus != CompanyDecisionStatus.Requested");
            }
            LastDecision = decision;
            
            var companyData = new CompanyLedger(Id, Name, ServiceLocator.Instance.FlowController.Month, ServiceLocator.Instance.FlowController.Year, LifetimeMonths);
            var averageWage = _jobContracts.Count > 0 ? _jobContracts.Select(x => x.Wage).Average() : 100;
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
            ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Commited);
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            hourglass.SetActive(false);
            Debug.Log("1B Decisions done.");
        }

       public void RequestMonthlyDecision()
       {
           Debug.Log("1A Decision requested.");
           ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Requested);
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
        
        public void EndMonth()
        {
            if (_writeToDatabase)
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
                    price = (double)LastDecision.PriceFood,
                    wage = (double)LastDecision.Wage,
                    sales = 1,
                    stock = ProductStockFood,
                    lifetime = LifetimeMonths,
                    sessionId = ServiceLocator.Instance.SessionId,
                    emergencyRounds = 0,
                    isStartup = false,
                    isTraining = _isTraining
                };
                StartCoroutine(HttpService.Insert("http://localhost:5000/companies/ledger", ledger));
            }

            

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
                    contract.QuitContract(WorkerFireReason.LackOfFunds);
                }
            }
            
            if (Liquidity < 10 || _jobContracts.Count == 0)
            {
                Reputation--;
                AddReward(-0.1F);
            }

            if (_jobContracts.Count > _lastWorkers)
            {
                AddReward(0.1F);
            }
            else if (_jobContracts.Count == _lastWorkers)
            {
                AddReward(0.01F);
            }
            else
            {
                AddReward(-0.11F);
            }

            //PaySocialFare();
            
            LifetimeMonths++;
            
            //UpdateCanvasText(false);
            AddReward(Reputation*0.01F);
            AddReward((float)Liquidity*0.01F);
            //AddReward(_jobContracts.Count*1F);
            if (Reputation >= 1000)
            {
                AddReward(1000F);
                //ProductStock = 0;
                //EndEpisode();
            }
            else if (Reputation <= -1000)
            {
                for (int i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    //var contract = _jobContracts[i];
                    //contract.QuitContract();
                }
                AddReward(-100F);
                //ProductStock = 0;
                //EndEpisode();
            }

            //int availableSpace = _jobContracts.Count * 400;
            int destroy = (int)(ProductStockFood * 0.1M);
            Ledger[^1].Food.Destroyed = destroy;
            ProductStockFood -= destroy;
            
            SetBuilding();

            Ledger[^1].Food.StockEndCheck = ProductStockFood;
            Ledger[^1].Luxury.StockEndCheck = ProductStockLuxury;
            Ledger[^1].Books.LiquidityEndCheck = Liquidity;
            Ledger[^1].Workers.EndCount = _jobContracts.Count;
            Ledger[^1].Reputation = Reputation;
            ServiceLocator.Instance.HouseholdAggregator.Add(Ledger[^1]);
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Pending);


            
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
            _lastWorkers = _jobContracts.Count;
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
                        worker.Give(societyShare);
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
