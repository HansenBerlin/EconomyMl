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
        public double OverallEmploymentRate => _overallEmploymentRate * 100;
        public double ShortTimeWorkingRate => _shortTimeWorkingRate * 100;
        public double FullyEmployedWorkingRate => _fullyEmployedWorkingRate * 100;
        public decimal AveragePriceBid { get; private set; }
        public decimal AverageFulltimeWage { get; private set; }
        public decimal AverageShortWorkWage { get; private set; }
        public decimal AverageReservationWage { get; private set; }
        public int AverageInventoryBeforeBuying { get; private set; }
        private int Population { get; set; }
        private int EmployedPopulation { get; set; }
        private double _overallEmploymentRate;
        private double _shortTimeWorkingRate;
        private double _fullyEmployedWorkingRate;
        
        public HouseholdsAggregate(int month, int year) : base(month, year) { }
        
        public void UpdateHouseholdData(HouseholdData data)
        {
            Population++;
            EmployedPopulation += data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0;
            AveragePurchasingPower = (AveragePurchasingPower * (Population - 1) + data.MoneyAvailableAdBidTime) / Population;
            AverageDemand = (AverageDemand * (Population - 1) + (data.Demand > 0 && data.PriceBid > 0 ? data.Demand : 0)) / Population;
            _overallEmploymentRate = (_overallEmploymentRate * (Population - 1) + (data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0)) / Population;
            _fullyEmployedWorkingRate = (_fullyEmployedWorkingRate * (Population - 1) + (data.JobStatus == WorkerJobStatus.FullyEmployed ? 1 : 0)) / Population;
            _shortTimeWorkingRate = (_shortTimeWorkingRate * (Population - 1) + (data.JobStatus == WorkerJobStatus.ShortTimeWork ? 1 : 0)) / Population;
            if (data.JobStatus == WorkerJobStatus.FullyEmployed)
            {
                AverageFulltimeWage = (AverageFulltimeWage * (EmployedPopulation - 1) + data.RealWage) / EmployedPopulation;
            }
            if (data.JobStatus == WorkerJobStatus.ShortTimeWork)
            {
                AverageShortWorkWage = (AverageShortWorkWage * (EmployedPopulation - 1) + data.RealWage / 2) / EmployedPopulation;
            }
            AveragePriceBid = (AveragePriceBid * (Population - 1) + (data.PriceBid > 0 && data.Demand > 0 ? data.PriceBid : 0)) / Population;
            AverageReservationWage = (AverageReservationWage * (Population - 1) + data.ReservationWage) / Population;
            AverageInventoryBeforeBuying = (AverageInventoryBeforeBuying * (Population - 1) + data.InventoryBeforeBuying) / Population;
        }
    }

    public class HouseholdData
    {
        public decimal MoneyAvailableAdBidTime { get; set; }
        public decimal PriceBid { get; set; }
        public decimal RealWage { get; set; }
        public decimal ReservationWage { get; set; }
        public int Demand { get; set; }
        public int InventoryBeforeBuying { get; set; }
        public WorkerJobStatus JobStatus { get; set; }
    }
}