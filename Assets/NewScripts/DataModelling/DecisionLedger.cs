using NewScripts.Game.Models;

namespace NewScripts.DataModelling
{
    public class DecisionLedger
    {
        public DecisionLedger(Decision decision)
        {
            FireWorkers = decision.WorkerChange < 0 ? decision.WorkerChange * -1 : 0;
            OpenPositions = decision.WorkerChange > 0 ? decision.WorkerChange : 0;
            SetWorkerWage = decision.Wage;
            SetFoodPrice = decision.PriceFood;
            SetLuxuryPrice = decision.PriceLuxury;
            ResourceDistribution = decision.RessourceDistribution;
        }

        public int FireWorkers { get; }
        public int OpenPositions { get; }
        public decimal SetWorkerWage { get; }
        public decimal SetFoodPrice { get; }
        public decimal SetLuxuryPrice { get; }
        public double ResourceDistribution { get; }
    }
}