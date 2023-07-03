using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Common;
using NewScripts.Enums;
using NewScripts.Http;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace NewScripts
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
        private int Reputation { get; set; }
        public int LifetimeMonths { get; private set; } = 1;
        public int Id => GetInstanceID();
        private readonly List<JobContract> _jobContracts = new();
        private bool _isTraining;
        private bool _writeToDatabase;
        public List<CompanyData> Ledger { get; } = new();
        public int WorkerCount => _jobContracts.Count;
        public decimal AverageWageRate => _jobContracts.Count == 0 ? 100 : _jobContracts.Average(x => x.Wage);
        public Decision LastDecision { get; private set; }

        
        private void Start()
        {
            _isTraining = ServiceLocator.Instance.Settings.IsTraining;
            _writeToDatabase = ServiceLocator.Instance.Settings.WriteToDatabase;
            //SetupAgent();
            Name = CompanyNames.PickRandomName();
        }

        private void Update() {  
            if (Input.GetMouseButtonDown(0)) {  
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            Reputation = 0;
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
            ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Requested);
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
                actionMask.SetActionEnabled(0, 0, false);
                actionMask.SetActionEnabled(0, 2, false);
            }
        }

        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            //UpdateCanvasText(false);
            if (DecisionStatus != CompanyDecisionStatus.Requested)
            {
                throw new System.Exception("DecisionStatus != CompanyDecisionStatus.Requested");
            }
            
            int workerDecision = actionBuffers.DiscreteActions[0];
            bool adaptWages = actionBuffers.DiscreteActions[1] == 1;
            float newWage = MapValue(actionBuffers.ContinuousActions[0], 10, 250);
            float foodPrice = MapValue(actionBuffers.ContinuousActions[1], 0.1F, 5F);
            float luxuryPrice = MapValue(actionBuffers.ContinuousActions[2], 1F, 100F);
            float ressourceDistribution = MapValue(actionBuffers.ContinuousActions[3], 0.1F, 1F);
            
            var companyData = new CompanyData(Id, ServiceLocator.Instance.FlowController.Month, 
                ServiceLocator.Instance.FlowController.Year, LifetimeMonths);
            var averageWage = _jobContracts.Count > 0 ? _jobContracts.Select(x => x.Wage).Average() : 100;
            var workerLedger = new WorkersLedger(_jobContracts.Count, (decimal)newWage, averageWage);
            var foodProductLedger = new ProductLedger((decimal)foodPrice, ProductStockFood);
            var luxuryProductLedger = new ProductLedger((decimal)luxuryPrice, ProductStockLuxury);
            var booksLedger = new BookKeepingLedger(Liquidity);
            int fireWorkers = (int)MapValue(actionBuffers.ContinuousActions[4], 1, _jobContracts.Count);
            int workerChange;
            
            if(workerDecision == 1)
            {
                OpenPositions = (int)MapValue(actionBuffers.ContinuousActions[5], 1, 1000);
                workerChange = OpenPositions;
            }
            else if(workerDecision == 2 && _jobContracts.Count > 0)
            {
                workerChange = fireWorkers * -1;
            }
            else
            {
                workerChange = 0;
            }

            LastDecision = new Decision((decimal) foodPrice, (decimal) luxuryPrice, ressourceDistribution, workerChange, (decimal) newWage, adaptWages);
            var decisionLedger = new DecisionLedger(LastDecision);

            if (adaptWages)
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
                    var jobBid = new JobBid(this, (decimal)newWage);
                    ServiceLocator.Instance.LaborMarket.AddJobBid(jobBid);
                    Academy.Instance.StatsRecorder.Add("Market/Job-Bid-Price", newWage);
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
                        Academy.Instance.StatsRecorder.Add("Labor/Fire", 1);

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
            ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Commited);
            //UpdateCanvasText(false);

        }

        private static float MapValue(float value, float minValue, float maxValue)
        {
            float mappedValue = (value + 1f) * 0.5f * (maxValue - minValue) + minValue;
            return mappedValue;
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
        
        public void EndMonth()
        {
            if (_writeToDatabase)
            {
                var ledger = new CompanyLedger
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

            

            for (var i = _jobContracts.Count - 1; i >= 0; i--)
            {
                var contract = _jobContracts[i];
                if (contract.Wage < Liquidity)
                {
                    contract.PayWorker();
                    Ledger[^1].Workers.ReducedPaidCount++;
                    Ledger[^1].Books.WagePayments += contract.Wage;
                    Reputation++;
                }
                else if (contract.Wage / 2 < Liquidity && contract.IsForceReduced == false)
                {
                    contract.PayReducedWage();
                    Ledger[^1].Workers.ReducedPaidCount++;
                    Ledger[^1].Books.WagePayments += contract.Wage / 2;
                    Reputation--;
                }
                else
                {
                    //contract.QuitContract(WorkerFireReason.LackOfFunds);
                    Reputation -= 2;
                }
            }
            
            if (Liquidity < 10 || _jobContracts.Count == 0)
            {
                Reputation--;
                AddReward(-0.1F);
            }

            

            //PaySocialFare();
            
            LifetimeMonths++;
            Academy.Instance.StatsRecorder.Add("Company/Liquidity", (float)Liquidity);
            Academy.Instance.StatsRecorder.Add("Company/SalesFood", Ledger.Last().Food.Sales);
            Academy.Instance.StatsRecorder.Add("Company/SalesLux", Ledger.Last().Luxury.Sales);
            //Academy.Instance.StatsRecorder.Add("Company/ProfitMonth", (float)Liquidity);
            Academy.Instance.StatsRecorder.Add("Company/Lifetime", LifetimeMonths);
            Academy.Instance.StatsRecorder.Add("Company/Rep", Reputation);
            Academy.Instance.StatsRecorder.Add("Product/PriceFood", (float)Ledger.Last().Food.PriceSet);
            Academy.Instance.StatsRecorder.Add("Product/PriceLux", (float)Ledger.Last().Luxury.PriceSet);
            Academy.Instance.StatsRecorder.Add("Product/Stock", ProductStockFood);
            Academy.Instance.StatsRecorder.Add("Labor/Workers", _jobContracts.Count);
            Academy.Instance.StatsRecorder.Add("Labor/Open", OpenPositions);
            
            //UpdateCanvasText(false);
            AddReward(Reputation*0.01F);
            //AddReward((float)Liquidity*0.01F);
            //int availableSpace = _jobContracts.Count * 400;
            int destroy = (int)(ProductStockFood * 0.1M);
            Ledger[^1].Food.Destroyed = destroy;
            ProductStockFood -= destroy;
            Ledger[^1].Food.StockEndCheck = ProductStockFood;
            Ledger[^1].Luxury.StockEndCheck = ProductStockLuxury;
            Ledger[^1].Books.LiquidityEndCheck = Liquidity;
            Ledger[^1].Workers.EndCount = _jobContracts.Count;
            Ledger[^1].Reputation = Reputation;
            if (Ledger.Count >= 2)
            {
                if (Ledger[^1].Books.LiquidityEndCheck > Ledger[^2].Books.LiquidityEndCheck)
                {
                    AddReward(0.1F);
                    Reputation += 1;
                }
                else
                {
                    AddReward(-0.1F);
                    Reputation -= 2;
                }
            }
            
            ServiceLocator.Instance.HouseholdAggregator.Add(Ledger[^1]);
            
            ServiceLocator.Instance.UiUpdateManager.BroadcastUpdateDecisionValuesEvent(this);
            ServiceLocator.Instance.FlowController.CommitDecision(Id, DecisionStatus = CompanyDecisionStatus.Pending);
            //AddReward(_jobContracts.Count*1F);
            if (Reputation >= 1000)
            {
                SetReward(100F);
                //ProductStock = 0;
                EndEpisode();
            }
            else if (Reputation <= -500)
            {
                for (int i = _jobContracts.Count - 1; i >= 0; i--)
                {
                    //var contract = _jobContracts[i];
                    //contract.QuitContract();
                }
                SetReward(-10F);
                //ProductStock = 0;
                EndEpisode();
            }
            
            SetBuilding();
            //ServiceLocator.Instance.FlowController.CommitDecision();
            //yield return new WaitForFixedUpdate();
            lastWorkers = _jobContracts.Count;
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
                    }
                    Liquidity = companyReserve + baseSafety;
                    Academy.Instance.StatsRecorder.Add("Labor/Social", (float)societyShare * unemployed.Count);
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
            AddReward(0.1F);
        }
        
        public void RemoveContract(JobContract contract, WorkerFireReason reason)
        {
            _jobContracts.Remove(contract);
            Reputation--;
            if (reason == WorkerFireReason.CompanyDecision)
            {
                Ledger[^1].Workers.FiredByDecision++;
                AddReward(-0.05F);
            }
            else if (reason == WorkerFireReason.LackOfFunds)
            {
                Ledger[^1].Workers.FiredByLackOfFunds++;
                AddReward(-0.1F);
            }
            else
            {
                Ledger[^1].Workers.Quit++;
                AddReward(-0.01F);
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
            ProductStockFood -= count;
            Liquidity += count * price;
            AddReward(0.01F);
        }
    }
}
