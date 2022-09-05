using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Enums;
using Factories;
using Models.Agents;
using Models.Finance;
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
        protected CreditRating CurrentRating { get; set; } = CreditRating.A;
        protected string Id { get; } = Guid.NewGuid().ToString();

        protected decimal Balance => BankAccount.Savings;
        protected decimal BalanceLastYear;
        protected GovernmentController Government;
        private CompanyDataRepository Data;
        protected IProductionTemplate Production;
        protected ICountryEconomy CountryEconomyMarkets;
        protected ProductController ProductController;
        protected NormalizationController NormCtr;

        protected BankAccountModel BankAccount;
        //protected CompanyResourcePolicy _policy;
        public ProductType TypeProduced => Production.TypeProduced;
        protected ProductType ResourceTypeNeeded => Production.ResourceTypeNeeded;
        protected ProductType EnergyTypeNeeded => Production.EnergyTypeNeeded;
        protected List<PersonAgent> Workers { get; } = new();
        
        protected long TotalDemand => CountryEconomyMarkets.GetTotalUnfulfilledDemand(TypeProduced);
        protected decimal MarketShare => CountryEconomyMarkets.MarketShare(TypeProduced, ProductController.Id);

        private int ObsProductionCapacityByWorkers => (int)(Production.UnitsPerWorker * ObserveTotalWorkers);

        private int ObservationPossibleProductionByResource => Production.ResourceNeededPerPiece > 0
            ? (int)(Production.AvailableProductionResources / Production.ResourceNeededPerPiece)
            : ObsProductionCapacityByWorkers;

        private int ObservationPossibleProductionByEnergy => Production.EnergyNeededPerPiece > 0
            ? (int)(Production.AvailableProductionEnergy / Production.EnergyNeededPerPiece)
            : ObsProductionCapacityByWorkers;

        protected int PossibleProduction => ObservationPossibleProductionByEnergy >= ObservationPossibleProductionByResource
            ? ObservationPossibleProductionByResource
            : ObservationPossibleProductionByEnergy;

        protected decimal ObserveTotalWorkers => Workers.Count + 1;

        protected decimal LastProdCostsInMonthForRessourcesAndEnergy;
        private decimal _lastWorkerPayments;
        protected long UnitsProducedInMonth;
        protected decimal ProfitTaxPaidInMonth;
        protected decimal ProfitAfterTaxesInMonth;
        protected decimal CashflowIn;
        protected decimal CashflowOut => TotalCostBeforeTaxes + ProfitTaxPaidInMonth;
        protected decimal UpgradeEffiencyCosts;
        protected decimal FixedPerProductBaseCosts => UnitsProducedInMonth * Production.BaseCostPerPieceProduced;
        private decimal CapacityUsed => (decimal)UnitsProducedInMonth / ObsProductionCapacityByWorkers;

        protected decimal TotalCostBeforeTaxes => FixedPerProductBaseCosts + _lastWorkerPayments + LastProdCostsInMonthForRessourcesAndEnergy + LoanPayments;

        protected decimal LoanPayments;
        protected decimal Cpp => UnitsProducedInMonth != 0
            ? TotalCostBeforeTaxes / UnitsProducedInMonth
            : ProductController.Price;

        protected JobMarketController JobMarket;
        //protected readonly List<JobModel> OpenJobPositions = new();


        public void Init(ICountryEconomy countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy, GovernmentController government, CompanyDataRepository data, 
            JobMarketController jobMarket, NormalizationController normController)
        {
            NormCtr = normController;
            CountryEconomyMarkets = countryEconomyMarkets;
            Government = government;
            Data = data;
            JobMarket = jobMarket;
            ProductController = productController;
            //_policy = policy;
            Production = productController.Template;
            ProductController.AddNew(policy.InitialResources);
            countryEconomyMarkets.ReportProduction(policy.InitialResources, TypeProduced);
            var openPositions = JobPositionFactory.Create(policy.InitialWorkers, policy.MinSalary, Id, Workers, TypeProduced);
            JobMarket.AddOpenJobPositions(openPositions);
            BankAccount = countryEconomyMarkets.OpenBankAccount(0, true);
            BankAccount.TryToGetLoan(policy.InitialBalance, CreditRating.AAA);
            BalanceLastYear = Balance;
            SetupObservations();
        }

        private void SetupObservations()
        {
            NormCtr.AddNew(nameof(Workers.Count), NormRange.One, Workers.Count);
            NormCtr.AddNew(nameof(ProductController.TotalSupply), NormRange.One, ProductController.TotalSupply);
            NormCtr.AddNew(nameof(ProductController.Price), NormRange.One, (float)ProductController.Price);
            NormCtr.AddNew(nameof(ProductController.Profit), NormRange.Two, (float)ProductController.Profit);
            NormCtr.AddNew(nameof(ProductController.ProfitLastMonth), NormRange.Two, (float)ProductController.ProfitLastMonth);
            NormCtr.AddNew(nameof(ProductController.SalesLastMonth), NormRange.One, ProductController.SalesLastMonth);
            NormCtr.AddNew(nameof(ProductController.SalesThisMonth), NormRange.One, ProductController.SalesThisMonth);
            NormCtr.AddNew(nameof(ProductController.ProductionThisMonth), NormRange.One, ProductController.ProductionThisMonth);
            NormCtr.AddNew(nameof(ProductController.ProductionLastMonth), NormRange.One, ProductController.ProductionLastMonth);
            NormCtr.AddNew(nameof(Production.AvailableProductionEnergy), NormRange.One, Production.AvailableProductionEnergy);
            NormCtr.AddNew(nameof(Production.AvailableProductionResources), NormRange.One, Production.AvailableProductionResources);
            NormCtr.AddNew(nameof(TotalDemand), NormRange.One, TotalDemand);
            NormCtr.AddNew(nameof(MarketShare), NormRange.One, (float)MarketShare);
            NormCtr.AddNew(nameof(BankAccount.LoansSum), NormRange.One, (float)BankAccount.LoansSum);
            NormCtr.AddNew(nameof(BankAccount.Savings), NormRange.Two, (float)BankAccount.Savings);
        }

        protected int _month;

        public void UpdateStats(int month)
        {
            _month = month;
            Data.BalanceStats.Add((double) Balance);
            Data.TotalProduced.Add(UnitsProducedInMonth);
            Data.WorkersStat.Add(Workers.Count);
            Data.MoneyOutStat.Add((double) CashflowOut);
            Data.MoneyInStat.Add((double) CashflowIn);
            ProductController.Update(EpisodeCut.Month, Cpp, CapacityUsed);
            UnitsProducedInMonth = 0;
            LastProdCostsInMonthForRessourcesAndEnergy = 0;
            _lastWorkerPayments = 0;
            ProfitTaxPaidInMonth = 0;
            ProfitAfterTaxesInMonth = 0;
            UpgradeEffiencyCosts = 0;
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
            else if (Balance + ProductController.Profit < 0)
            {
                JobMarket.RemoveOpenJobPositions(0, Id, true);
                FireWorkers(Workers.Count);
                SetReward(-1);
                EndEpisode();
                CountryEconomyMarkets.RemoveBusiness(this, ProductController.Id);
                return true;
            }

            return false;
        }

        protected void FireWorkers(int count)
        {
            int keep = Workers.Count < count ? 0 : Workers.Count - count;
            while (Workers.Count > keep)
            {
                var w = Workers[0];
                w.Fire();
                if (count-- <= 0)
                {
                    break;
                }
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
                BankAccount.Withdraw(paid + incomeTax);
                _lastWorkerPayments += paid + incomeTax;
            }
        }


        public abstract void MonthlyBookkeeping();
        public abstract void ActionAdaptPrices(float newPriceMultiplier);

    }
}