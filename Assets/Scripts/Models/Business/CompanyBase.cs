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

namespace Models.Business
{
    public abstract class CompanyBase
    {
        public string Id { get; } = Guid.NewGuid().ToString();

        protected decimal Balance;
        protected readonly GovernmentController Government;

        private readonly CompanyDataRepository Data;

        //protected readonly List<IPersonBase> Workers = new();
        protected readonly IProductionTemplate Production;

        //protected ProductController ProductController { get; }
        protected readonly ICountryEconomy CountryEconomyMarkets;
        protected readonly ProductController ProductController;
        protected readonly CompanyResourcePolicy _policy;
        public ProductType TypeProduced => Production.TypeProduced;
        public ProductType ResourceTypeNeeded => Production.ResourceTypeNeeded;
        public ProductType EnergyTypeNeeded => Production.EnergyTypeNeeded;
        protected List<PersonAgent> Workers { get; } = new();
        public long EstimatedEnergyDemand => (long) (Production.EnergyNeededPerPiece * ObsProductionCapacityByWorkers);

        public long EstimatedResourceDemand =>
            (long) (Production.ResourceNeededPerPiece * ObsProductionCapacityByWorkers);

        private decimal ObsProductionCapacityByWorkers =>
            Production.UnitsPerWorker * ObserveTotalWorkers * Production.WorkerEfficiencyMultiplier;

        private decimal ObservationPossibleProductionByResource => Production.ResourceNeededPerPiece > 0
            ? Production.AvailableProductionResources / Production.ResourceNeededPerPiece
            : ObsProductionCapacityByWorkers;

        private decimal ObservationPossibleProductionByEnergy => Production.EnergyNeededPerPiece > 0
            ? Production.AvailableProductionEnergy / Production.EnergyNeededPerPiece
            : ObsProductionCapacityByWorkers;

        protected decimal ObservationTotalResourceDemandPerMonth => CalculateDemandForMonthlyProduction("r");
        protected decimal ObservationTotalEnergyDemandPerMonth => CalculateDemandForMonthlyProduction("e");

        protected decimal ObserveTotalWorkers => Workers.Count + 1;

        protected decimal LastProdCostsInMonthForRessourcesAndEnergy;
        protected decimal _lastWorkerPayments;
        protected long _unitsProducedInMonth;
        protected decimal ProfitTaxPaidInMonth;
        protected decimal ProfitAfterTaxesInMonth;
        protected decimal CashflowIn;
        private decimal CashflowOut => TotalCostBeforeTaxes + ProfitTaxPaidInMonth + UpgradeEffiencyCosts;
        protected decimal UpgradeEffiencyCosts;
        protected int MissingResourceDemand;
        protected decimal FixedPerProductCosts => _unitsProducedInMonth * Production.BaseCostPerPieceProduced;
        protected decimal CapacityUsed => _unitsProducedInMonth / ObsProductionCapacityByWorkers;

        protected decimal TotalCostBeforeTaxes =>
            FixedPerProductCosts + _lastWorkerPayments + LastProdCostsInMonthForRessourcesAndEnergy;

        protected decimal Cpp => _unitsProducedInMonth != 0
            ? TotalCostBeforeTaxes / _unitsProducedInMonth
            : ProductController.Price * 0.8M;

        protected readonly JobMarketController JobMarket;
        //protected readonly List<JobModel> OpenJobPositions = new();


        protected CompanyBase(ICountryEconomy countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy,
            GovernmentController government, CompanyDataRepository data, JobMarketController jobMarket)
        {
            CountryEconomyMarkets = countryEconomyMarkets;
            Balance = policy.InitialBalance;
            Government = government;
            Data = data;
            JobMarket = jobMarket;
            ProductController = productController;
            _policy = policy;
            Production = productController.Template;
            ProductController.AddNew(policy.InitialResources);
            countryEconomyMarkets.ReportProduction(policy.InitialResources, TypeProduced);
            var openPositions = JobPositionFactory.Create(policy.InitialWorkers, policy.MinSalary, Id, Workers, TypeProduced);
            jobMarket.AddOpenJobPositions(openPositions);
        }

        private decimal lastCpp = 0;

        public void Reset(int month)
        {
            if (Cpp > lastCpp * 1.5M)
                Console.WriteLine(month);
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
        }

        public bool IsRemoved()
        {
            if (Balance + ProductController.Profit < 0)
            {
                FireWorkers(Workers.Count);
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

        public abstract void ActionInvestInEfficiency();

        private decimal CalculateDemandForMonthlyProduction(string type)
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

        public abstract void ActionBuyResources(int daysLeft);

        public abstract void ActionBuyEnergy(int daysLeft);




        public void ActionProduce()
        {
            decimal unitsProduced = ObservationPossibleProductionByEnergy >= ObservationPossibleProductionByResource
                ? ObservationPossibleProductionByResource
                : ObservationPossibleProductionByEnergy;

            int finalProduction = (int) (unitsProduced * Production.MachineEfficiencyMultiplier);
            ProductController.AddNew(finalProduction);
            _unitsProducedInMonth += finalProduction;

            CountryEconomyMarkets.ReportProduction(finalProduction, TypeProduced);

            Production.AvailableProductionEnergy -= (int)(unitsProduced * Production.EnergyNeededPerPiece);
            Production.AvailableProductionResources -= (int)(unitsProduced * Production.ResourceNeededPerPiece);
        }


        public void QuarterlyUpdate()
        {
            ProductController.Update(EpisodeCut.Quarter);
        }


        public abstract void ActionAdaptProductionCapacity();







        protected decimal GetWorkforcePayments()
        {
            return Workers.Sum(w => w.MonthlyIncome);
        }



        protected void PayWorkers()
        {
            foreach (var w in Workers)
            {
                decimal paid = w.MonthlyIncome;
                decimal incomeTax = Government.PayIncomeTax(paid);
                Balance -= paid + incomeTax;
                _lastWorkerPayments += paid + incomeTax;
            }
        }


        public abstract void MonthlyBookkeeping();
        public abstract void ActionAdaptPrices();

    }
}