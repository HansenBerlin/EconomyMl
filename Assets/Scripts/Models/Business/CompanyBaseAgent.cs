using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Enums;
using Factories;
using Models.Agents;
using Models.Market;
using Models.Production;
using Policies;
using Repositories;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

namespace Models.Business
{



    public abstract class CompanyBaseAgent : Agent
    {
        public string Id { get; } = Guid.NewGuid().ToString();

        protected decimal Balance;
        protected decimal BalanceLastYear;
        protected GovernmentController Government;
        protected CompanyDataRepository Data;
        protected IProductionTemplate Production;
        protected ICountryEconomy CountryEconomyMarkets;
        protected ProductController ProductController;
        //protected CompanyResourcePolicy _policy;
        public ProductType TypeProduced => Production.TypeProduced;
        public ProductType ResourceTypeNeeded => Production.ResourceTypeNeeded;
        public ProductType EnergyTypeNeeded => Production.EnergyTypeNeeded;
        protected List<PersonAgent> Workers { get; } = new();
        
        protected int ObsProductionCapacityByWorkers => (int)(Production.UnitsPerWorker * ObserveTotalWorkers);
        protected int ObservationPossibleProductionByResource => Production.ResourceNeededPerPiece > 0
            ? (int)(Production.AvailableProductionResources / Production.ResourceNeededPerPiece)
            : ObsProductionCapacityByWorkers;
        protected int ObservationPossibleProductionByEnergy => Production.EnergyNeededPerPiece > 0
            ? (int)(Production.AvailableProductionEnergy / Production.EnergyNeededPerPiece)
            : ObsProductionCapacityByWorkers;

        protected int PossibleProduction => ObservationPossibleProductionByEnergy >= ObservationPossibleProductionByResource
            ? ObservationPossibleProductionByResource
            : ObservationPossibleProductionByEnergy;

        protected decimal ObserveTotalWorkers => Workers.Count + 1;

        protected decimal LastProdCostsInMonthForRessourcesAndEnergy;
        protected decimal _lastWorkerPayments;
        protected long _unitsProducedInMonth;
        protected decimal ProfitTaxPaidInMonth;
        protected decimal ProfitAfterTaxesInMonth;
        protected decimal CashflowIn;
        protected decimal CashflowOut => TotalCostBeforeTaxes + ProfitTaxPaidInMonth;
        protected decimal UpgradeEffiencyCosts;
        protected int MissingResourceDemand;
        protected decimal FixedPerProductBaseCosts => _unitsProducedInMonth * Production.BaseCostPerPieceProduced;
        protected decimal CapacityUsed => (decimal)_unitsProducedInMonth / ObsProductionCapacityByWorkers;

        protected decimal TotalCostBeforeTaxes => FixedPerProductBaseCosts + _lastWorkerPayments + LastProdCostsInMonthForRessourcesAndEnergy + LoanPayments;

        protected decimal LoanPayments;
        protected decimal Cpp => _unitsProducedInMonth != 0
            ? TotalCostBeforeTaxes / _unitsProducedInMonth
            : ProductController.Price;

        protected JobMarketController JobMarket;
        //protected readonly List<JobModel> OpenJobPositions = new();


        public void Init(ICountryEconomy countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy, GovernmentController government, CompanyDataRepository data, JobMarketController jobMarket)
        {
            CountryEconomyMarkets = countryEconomyMarkets;
            Balance = policy.InitialBalance;
            BalanceLastYear = Balance;
            Government = government;
            Data = data;
            JobMarket = jobMarket;
            ProductController = productController;
            //_policy = policy;
            Production = productController.Template;
            ProductController.AddNew(policy.InitialResources);
            countryEconomyMarkets.ReportProduction(policy.InitialResources, TypeProduced);
            var openPositions = JobPositionFactory.Create(policy.InitialWorkers, policy.MinSalary, Id, Workers, TypeProduced);
            jobMarket.AddOpenJobPositions(openPositions);
        }

        private decimal lastCpp = 0;
        protected int _month;

        public void UpdateStats(int month)
        {
            _month = month;
            lastCpp = Cpp;
            Data.BalanceStats.Add((double) Balance);
            Data.TotalProduced.Add(_unitsProducedInMonth);
            Data.WorkersStat.Add(Workers.Count);
            Data.MoneyOutStat.Add((double) CashflowOut);
            Data.MoneyInStat.Add((double) CashflowIn);
            ProductController.Update(EpisodeCut.Month, Cpp, CapacityUsed);

            
            
            
            
            _unitsProducedInMonth = 0;
            LastProdCostsInMonthForRessourcesAndEnergy = 0;
            _lastWorkerPayments = 0;
            ProfitTaxPaidInMonth = 0;
            ProfitAfterTaxesInMonth = 0;
            UpgradeEffiencyCosts = 0;
            MissingResourceDemand = 0;
            CashflowIn = 0;
            LoanPayments = 0;
            
        }

        public abstract void MakeDecision(CompanyActionPhase phase);
        public abstract void EndYear(CompanyActionPhase phase);


        public bool IsRemoved()
        {
            if (TypeProduced == ProductType.FederalService)
            {
                return false;
            }
            if (Balance + ProductController.Profit < 0)
            {
                JobMarket.RemoveOpenJobPositions(0, Id, true);
                FireWorkers(Workers.Count);
                AddReward(-2);
                EndEpisode();
                CountryEconomyMarkets.RemoveBusiness(this, ProductController.Id);
                return true;
            }

            return false;
        }

        protected void FireWorkers(int count)
        {
            int keep = Workers.Count - count;
            while (Workers.Count > keep)
            {
                var w = Workers[0];
                w.Fire();
                if (--count <= 0)
                    break;
            }
        }


        public decimal CalculateDemandForMonthlyProduction(string type)
        {
            if (type == "r")
            {
                decimal demand = Production.ResourceNeededPerPiece > 0
                    ? ObsProductionCapacityByWorkers * Production.ResourceNeededPerPiece -
                      Production.AvailableProductionResources
                    : 0;
                return demand > 0 ? demand : 0;
            }
            else
            {
                decimal demand = Production.EnergyNeededPerPiece > 0
                    ? ObsProductionCapacityByWorkers * Production.EnergyNeededPerPiece -
                      Production.AvailableProductionEnergy
                    : 0;
                return demand > 0 ? demand : 0;
            }
        }

        public abstract void ActionBuyResources(decimal maxSpendings, long resourcesDemanded);

        public abstract void ActionBuyEnergy(decimal maxSpendings, long energyDemanded);
        public abstract void ActionProduce(int percentProduction);


        


        public void QuarterlyUpdate()
        {
            ProductController.Update(EpisodeCut.Quarter);
        }


        public abstract void ActionAdaptProductionCapacity(float changeProductionCapabilities, int maxSalary);







        protected decimal GetWorkforcePayments()
        {
            return Workers.Sum(w => w.MonthlyIncome);
        }

        private float minC;
        private float maxC;
        
        protected float Normalize(float value)
        {
            minC = value < minC ? value : minC;
            maxC = value > maxC ? value : maxC;
            if (maxC - minC == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - minC) / (maxC - minC)) - 1;
            return norm;
        }



        protected void PayWorkers()
        {
            foreach (var w in Workers)
            {
                decimal paid = w.Pay();
                decimal incomeTax = Government.PayIncomeTax(paid);
                Balance -= paid + incomeTax;
                _lastWorkerPayments += paid + incomeTax;
            }
        }


        public abstract void MonthlyBookkeeping();
        public abstract void ActionAdaptPrices(float newPriceMultiplier);

    }
}