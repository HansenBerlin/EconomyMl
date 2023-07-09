using System.Collections.Generic;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;

namespace NewScripts.Interfaces
{
    public interface ICompany
    {
        int Id { get; }
        string Name { get; }
        decimal Liquidity { get; set; }
        decimal AverageWageRate { get; }
        Decision LastDecision { get; }
        //int ProductStockFood { get; }
        int LifetimeMonths { get; }
        int WorkerCount { get; }
        PlayerType PlayerType { get; }
        CompanyDecisionStatus DecisionStatus { get; }
        List<CompanyLedger> Ledger { get; }
        void FullfillBid(ProductType product, int count, decimal price);
        void AddContract(JobContract contract);
        void RemoveContract(JobContract contract, WorkerFireReason reason);
        void RequestMonthlyDecision();
        void StartNextPeriod(Decision decision);
        void Produce();
        void EndMonth(double lastBidProceFood, int lastDemandFood);
        void AddRewards(int year);
    }
}
