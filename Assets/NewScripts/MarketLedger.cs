using Enums;
using NewScripts.Enums;
using UnityEngine;

namespace NewScripts
{
    public class Aggregate
    {
        protected Aggregate(int month, int year)
        {
            Month = month;
            Year = year;
        }

        public int Month { get; }
        public int Year { get; }
    }
    
    public class CompaniesAggregate : Aggregate
    {
        public decimal AverageWageOffer { get; private set; }
        public decimal AveragePriceOffer { get; private set; }
        public decimal AverageLiquidity { get; private set; }
        public double AverageSupply { get; private set; }
        public int AverageStock { get; private set; }
        public int AverageReputation { get; private set; }
        public int AverageLifetime { get; private set; }
        public int AverageSales { get; private set; }
        public int AverageOpenPositions { get; private set; }
        public int AverageFiredWorkersTotal { get; private set; }
        public int AverageFiredWorkersByDecision { get; private set; }
        public int AverageFiredWorkersByLackOfFunds { get; private set; }
        private int Companys { get; set; }
        
        public CompaniesAggregate(int month, int year) : base(month, year) { }
        
        public void UpdateCompanyData(CompanyData data)
        {
            Companys++;
            AverageWageOffer = (AverageWageOffer * (Companys - 1) + data.Workers.OfferedWage) / Companys;
            AveragePriceOffer = (AveragePriceOffer * (Companys - 1) + data.Product.PriceSet) / Companys;
            AverageLiquidity = (AverageLiquidity * (Companys - 1) + data.Books.LiquidityStart) / Companys;
            AverageSupply = (AverageSupply * (Companys - 1) + data.Product.Production) / Companys;
            AverageStock = (AverageStock * (Companys - 1) + data.Product.StockStart) / Companys;
            AverageReputation = (AverageReputation * (Companys - 1) + data.Reputation) / Companys;
            AverageLifetime = (AverageLifetime * (Companys - 1) + data.Lifetime) / Companys;
            AverageSales = (AverageSales * (Companys - 1) + data.Product.Sales) / Companys;
            AverageOpenPositions = (AverageOpenPositions * (Companys - 1) + data.Workers.OpenPositions) / Companys;
            AverageFiredWorkersTotal = (AverageFiredWorkersTotal * (Companys - 1) + data.Workers.FiredByDecision + data.Workers.FiredByLackOfFunds) / Companys;
            AverageFiredWorkersByDecision = (AverageFiredWorkersByDecision * (Companys - 1) + data.Workers.FiredByDecision) / Companys;
            AverageFiredWorkersByLackOfFunds = (AverageFiredWorkersByLackOfFunds * (Companys - 1) + data.Workers.FiredByLackOfFunds) / Companys;
        }
    }
    
    public class HouseholdsAggregate : Aggregate
    {
        public decimal AveragePurchasingPower { get; private set; }
        public double AverageDemand { get; private set; }
        public double OverallEmploymentRate { get; private set; }
        public double ShortTimWorkingRate { get; private set; }
        public double FullyEmployedWorkingRate { get; private set; }
        public decimal AveragePriceBid { get; private set; }
        public decimal AverageFulltimeWage { get; private set; }
        public decimal AverageShortWorkWage { get; private set; }
        public decimal AverageReservationWage { get; private set; }
        public int AverageInventoryBeforeBuying { get; private set; }
        private int Population { get; set; }
        private int EmployedPopulation { get; set; }
        
        public HouseholdsAggregate(int month, int year) : base(month, year) { }
        
        public void UpdateHouseholdData(HouseholdData data)
        {
            Population++;
            EmployedPopulation += data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0;
            AveragePurchasingPower = (AveragePurchasingPower * (Population - 1) + data.MoneyAvailableAdBidTime) / Population;
            AverageDemand = (AverageDemand * (Population - 1) + (data.Demand > 0 && data.PriceBid > 0 ? data.Demand : 0)) / Population;
            OverallEmploymentRate = (OverallEmploymentRate * (Population - 1) + (data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0)) / Population;
            ShortTimWorkingRate = (ShortTimWorkingRate * (EmployedPopulation - 1) + (data.JobStatus == WorkerJobStatus.ShortTimeWork ? 1 : 0)) / Population;
            FullyEmployedWorkingRate = (FullyEmployedWorkingRate * (EmployedPopulation - 1) + (data.JobStatus == WorkerJobStatus.FullyEmployed ? 1 : 0)) / Population;
            AveragePriceBid = (AveragePriceBid * (Population - 1) + (data.PriceBid > 0 && data.Demand > 0 ? data.PriceBid : 0)) / Population;
            AverageFulltimeWage = data.JobStatus == WorkerJobStatus.FullyEmployed ? (AverageFulltimeWage * (EmployedPopulation - 1) + data.RealWage) / EmployedPopulation : AverageFulltimeWage;
            AverageShortWorkWage = data.JobStatus == WorkerJobStatus.ShortTimeWork ? (AverageShortWorkWage * (EmployedPopulation - 1) + data.RealWage) / EmployedPopulation : AverageShortWorkWage;
            AverageReservationWage = (AverageReservationWage * (Population - 1) + data.ReservationWage) / Population;
            AverageInventoryBeforeBuying = (AverageInventoryBeforeBuying * (Population - 1) + data.InventoryBeforeBuying) / Population;
        }
    }

    public class HouseholdData
    {
        public decimal MoneyAvailableAdBidTime { get; }
        public decimal PriceBid { get; }
        public decimal RealWage { get; set; }
        public decimal ReservationWage { get; set; }
        public int Demand { get; }
        public int InventoryBeforeBuying { get; }
        public WorkerJobStatus JobStatus { get; set; }
        
        
        public HouseholdData(int demand, decimal moneyAvailableAdBidTime, int inventoryBeforeBuying, decimal priceBid)
        {
            Demand = demand;
            MoneyAvailableAdBidTime = moneyAvailableAdBidTime;
            InventoryBeforeBuying = inventoryBeforeBuying;
            PriceBid = priceBid;
        }
    }
}