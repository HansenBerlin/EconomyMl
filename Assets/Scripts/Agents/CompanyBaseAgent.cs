using System;
using System.Collections.Generic;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Factories;
using Interfaces;
using Models;
using Policies;
using Repositories;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

namespace Agents
{
    public abstract class CompanyBaseAgent : Agent
    {
        private CompanyDataRepository _data;
        private float _maxC;
        private float _minC;
        protected decimal BalanceLastYear;
        protected BankAccountModel BankAccount;
        protected decimal CashflowIn;
        protected ICountryEconomy CountryEconomyMarkets;
        protected GovernmentAgent Government;

        protected JobMarketController JobMarket;

        protected decimal LastProdCostsInMonthForRessourcesAndEnergy;

        private decimal _lastWorkerPayments;
        protected decimal LoanPayments;
        protected NormalizationController NormCtr;
        protected ProductController ProductController;
        protected IProductionTemplate Production;
        protected decimal ProfitTaxPaidInMonth;
        protected long UnitsProducedInMonth;
        protected decimal UpgradeEffiencyCosts;
        protected CreditRating CurrentRating { get; set; } = CreditRating.A;
        protected string Id { get; } = Guid.NewGuid().ToString();

        protected decimal Balance => BankAccount.Savings;

        public ProductType TypeProduced => Production.TypeProduced;
        protected ProductType ResourceTypeNeeded => Production.ResourceTypeNeeded;
        protected ProductType EnergyTypeNeeded => Production.EnergyTypeNeeded;
        protected List<PersonAgent> Workers { get; } = new();

        protected long TotalDemand => CountryEconomyMarkets.GetTotalUnfulfilledDemand(TypeProduced);
        protected decimal MarketShare => CountryEconomyMarkets.MarketShare(TypeProduced, ProductController.Id);

        private int ObsProductionCapacityByWorkers => (int) (Production.UnitsPerWorker * ObserveTotalWorkers);

        private int ObservationPossibleProductionByResource => Production.ResourceNeededPerPiece > 0
            ? (int) (Production.AvailableProductionResources / Production.ResourceNeededPerPiece)
            : ObsProductionCapacityByWorkers;

        private int ObservationPossibleProductionByEnergy => Production.EnergyNeededPerPiece > 0
            ? (int) (Production.AvailableProductionEnergy / Production.EnergyNeededPerPiece)
            : ObsProductionCapacityByWorkers;

        protected int PossibleProduction =>
            ObservationPossibleProductionByEnergy >= ObservationPossibleProductionByResource
                ? ObservationPossibleProductionByResource
                : ObservationPossibleProductionByEnergy;

        private decimal ObserveTotalWorkers => Workers.Count + 1;
        protected decimal CashflowOut => TotalCostBeforeTaxes + ProfitTaxPaidInMonth + LoanPayments;
        protected decimal FixedPerProductBaseCosts => UnitsProducedInMonth * Production.BaseCostPerPieceProduced;
        private decimal CapacityUsed => (decimal) UnitsProducedInMonth / ObsProductionCapacityByWorkers;

        protected decimal TotalCostBeforeTaxes => FixedPerProductBaseCosts + _lastWorkerPayments +
                                                  LastProdCostsInMonthForRessourcesAndEnergy;

        protected decimal Cpp => UnitsProducedInMonth != 0
            ? TotalCostBeforeTaxes / UnitsProducedInMonth
            : ProductController.Price;


        public void Init(ICountryEconomy countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy, GovernmentAgent government, CompanyDataRepository data,
            JobMarketController jobMarket, NormalizationController normController)
        {
            NormCtr = normController;
            CountryEconomyMarkets = countryEconomyMarkets;
            Government = government;
            _data = data;
            JobMarket = jobMarket;
            ProductController = productController;
            Production = productController.Template;
            ProductController.AddNew(policy.InitialResources);
            var openPositions = JobPositionFactory.Create(policy.InitialWorkers, policy.MinSalary, Id, Workers, TypeProduced);
            JobMarket.AddOpenJobPositions(openPositions);
            if (TypeProduced != ProductType.FederalService)
            {
                BankAccount = countryEconomyMarkets.OpenBankAccount(0, true);
                BankAccount.IsLoanAdded(policy.InitialBalance, CreditRating.Aaa);
            }
            else
            {
                BankAccount = countryEconomyMarkets.OpenBankAccount(policy.InitialBalance, true);
            }

            BalanceLastYear = Balance;
            SetupObservations();
        }

        private void SetupObservations()
        {
            NormCtr.AddNew(nameof(Workers.Count), NormRange.One, Workers.Count);
            NormCtr.AddNew(nameof(ProductController.TotalSupply), NormRange.One, ProductController.TotalSupply);
            NormCtr.AddNew(nameof(ProductController.Price), NormRange.One, (float) ProductController.Price);
            NormCtr.AddNew(nameof(ProductController.Profit), NormRange.Two, (float) ProductController.Profit);
            NormCtr.AddNew(nameof(ProductController.ProfitLastMonth), NormRange.Two,
                (float) ProductController.ProfitLastMonth);
            NormCtr.AddNew(nameof(ProductController.SalesLastMonth), NormRange.One, ProductController.SalesLastMonth);
            NormCtr.AddNew(nameof(ProductController.SalesThisMonth), NormRange.One, ProductController.SalesThisMonth);
            NormCtr.AddNew(nameof(ProductController.ProductionThisMonth), NormRange.One,
                ProductController.ProductionThisMonth);
            NormCtr.AddNew(nameof(ProductController.ProductionLastMonth), NormRange.One,
                ProductController.ProductionLastMonth);
            NormCtr.AddNew(nameof(Production.AvailableProductionEnergy), NormRange.One,
                Production.AvailableProductionEnergy);
            NormCtr.AddNew(nameof(Production.AvailableProductionResources), NormRange.One,
                Production.AvailableProductionResources);
            NormCtr.AddNew(nameof(TotalDemand), NormRange.One, TotalDemand);
            NormCtr.AddNew(nameof(MarketShare), NormRange.One, (float) MarketShare);
            NormCtr.AddNew(nameof(BankAccount.LoansSum), NormRange.One, (float) BankAccount.LoansSum);
            NormCtr.AddNew(nameof(BankAccount.Savings), NormRange.Two, (float) BankAccount.Savings);
        }

        public void UpdateStats(int month)
        {
            _data.BalanceStats.Add((double) Balance);
            _data.TotalProduced.Add(UnitsProducedInMonth);
            _data.WorkersStat.Add(Workers.Count);
            _data.MoneyOutStat.Add((double) CashflowOut);
            _data.MoneyInStat.Add((double) CashflowIn);
            ProductController.Update(EpisodeCut.Month, Cpp, CapacityUsed);
            UnitsProducedInMonth = 0;
            LastProdCostsInMonthForRessourcesAndEnergy = 0;
            _lastWorkerPayments = 0;
            ProfitTaxPaidInMonth = 0;
            UpgradeEffiencyCosts = 0;
            CashflowIn = 0;
            LoanPayments = 0;
        }

        public abstract void MakeDecision(CompanyActionPhase phase);
        public abstract void EndYear(CompanyActionPhase phase);

        public bool IsRemoved()
        {
            if (TypeProduced == ProductType.FederalService) return false;

            if (Balance + ProductController.Profit < 0)
            {
                JobMarket.RemoveOpenJobPositions(0, Id, true);
                FireWorkers(Workers.Count);
                SetReward(-1);
                EndEpisode();
                BankAccount.CloseAccount();
                CountryEconomyMarkets.RemoveBusiness(this, ProductController.Id);
                Destroy(gameObject);
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
                if (count-- <= 0) break;
            }
        }


        protected decimal CalculateDemandForMonthlyProduction(string type)
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

        protected float Normalize(float value)
        {
            _minC = value < _minC ? value : _minC;
            _maxC = value > _maxC ? value : _maxC;
            if (_maxC - _minC == 0) return 0;
            float norm = 2 * ((value - _minC) / (_maxC - _minC)) - 1;
            return norm;
        }


        protected void PayWorkers()
        {
            Debug.Log(TypeProduced.ToString());
            foreach (var w in Workers)
            {
                decimal paid = w.Pay();
                decimal incomeTax = Government.PayIncomeTax(paid);
                BankAccount.Withdraw(paid + incomeTax);
                _lastWorkerPayments += paid + incomeTax;
            }
        }

        public abstract void MonthlyBookkeeping(int month);
    }
}