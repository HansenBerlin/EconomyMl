using System.Collections.Generic;
using NewScripts.DataModelling;
using NewScripts.Game.Flow;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Game.Services
{
    public class HouseholdAggregatorService : MonoBehaviour
    {
        [FormerlySerializedAs("periodAggregateAddedEvent")] public PeriodAggregateAddedEvent periodHouseholdAggregateAddedEvent = new();
        public PeriodAggregateAddedEvent periodCompanyAggregateAddedEvent = new();
        public List<HouseholdsAggregate> HouseholdsAggregates { get; } = new();
        public List<CompaniesAggregate> CompaniesAggregates { get; } = new();

        private void Awake()
        {
            if (HouseholdsAggregates.Count == 0)
            {
                HouseholdsAggregates.Add(new HouseholdsAggregate(1, 1));
            }
            if (CompaniesAggregates.Count == 0)
            {
                CompaniesAggregates.Add(new CompaniesAggregate(1, 1));
            }
        }

        public void Add(HouseholdData data)
        {
            HouseholdsAggregates[^1].UpdateHouseholdData(data);
        }
        
        public void Add(CompanyLedger ledger)
        {
            CompaniesAggregates[^1].UpdateCompanyData(ledger);
        }

        public void StartNewPeriod(int month, int year)
        {
            if (HouseholdsAggregates.Count > 120)
            {
                HouseholdsAggregates.RemoveRange(0, HouseholdsAggregates.Count - 120);
                CompaniesAggregates.RemoveRange(0, CompaniesAggregates.Count - 120);
            }
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/DemandFood", (float)HouseholdsAggregates[^1].AverageDemandFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/DemandLux", (float)HouseholdsAggregates[^1].AverageDemandLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/FulltimeWage", (float)HouseholdsAggregates[^1].AverageFulltimeWage);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/ResrvationtimeWage", (float)HouseholdsAggregates[^1].AverageReservationWage);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/PurchasingPower", (float)HouseholdsAggregates[^1].AveragePurchasingPower);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/EmploymentRate", (float)HouseholdsAggregates[^1].OverallEmploymentRate);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/BidFood", (float)HouseholdsAggregates[^1].AveragePriceBidFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/BidLux", (float)HouseholdsAggregates[^1].AveragePriceBidLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/Inv-Food", HouseholdsAggregates[^1].AverageFoodInventoryBeforeBuying);
            Academy.Instance.StatsRecorder.Add("Aggregates-HH/Inv-Lux", HouseholdsAggregates[^1].AverageLuxuryInventoryBeforeBuying);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/Lifetime", CompaniesAggregates[^1].AverageLifetime);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/Liquidity", (float)CompaniesAggregates[^1].AverageLiquidity);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/Reputation", CompaniesAggregates[^1].AverageReputation);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/OpenPositions", CompaniesAggregates[^1].AverageOpenPositions);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/SalesFood", CompaniesAggregates[^1].AverageSalesFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/SalesLux", CompaniesAggregates[^1].AverageSalesLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/StockFood", CompaniesAggregates[^1].AverageStockFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/StockLux", CompaniesAggregates[^1].AverageStockLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/SupplyFood", (float)CompaniesAggregates[^1].AverageSupplyFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/SupplyLux", (float)CompaniesAggregates[^1].AverageSupplyLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/WageOffer", (float)CompaniesAggregates[^1].AverageWageOffer);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/PriceFood", (float)CompaniesAggregates[^1].AveragePriceOfferFood);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/PriceLux", (float)CompaniesAggregates[^1].AveragePriceOfferLuxury);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/WorkersFired", CompaniesAggregates[^1].AverageFiredWorkersTotal);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/WorkersFiredByDecision", CompaniesAggregates[^1].AverageFiredWorkersByDecision);
            Academy.Instance.StatsRecorder.Add("Aggregates-CO/WorkersFiredByForce", CompaniesAggregates[^1].AverageFiredWorkersByLackOfFunds);
            

            periodHouseholdAggregateAddedEvent.Invoke(HouseholdsAggregates[^1]);
            periodCompanyAggregateAddedEvent.Invoke(CompaniesAggregates[^1]);
            HouseholdsAggregates.Add(new HouseholdsAggregate(month, year));
            CompaniesAggregates.Add(new CompaniesAggregate(month, year));
        }
    }
}