using Enums;

namespace Controller.Agents
{
    public static class RatingController
    {
        public static CreditRating Calculate(decimal balance, decimal profitTrend, decimal loansTakenSum,
            decimal lastMonthProfit, CreditRating currentRating)
        {
            int balanceFactor = balance < -100000 ? -2 : balance <= 0 ? -1 : balance > 1000000 ? +2 : 1;
            int profitFactor = profitTrend < -0.5M ? -2 : profitTrend <= 0 ? -1 : profitTrend > 0.5M ? 2 : 1;
            decimal monthlyAvailableCapital = lastMonthProfit - loansTakenSum / 12;
            int loansFactor = monthlyAvailableCapital > 0 ? 1 : -1;
            int combinedFactor = balanceFactor + profitFactor + loansFactor;
            var newRating = (int) currentRating + combinedFactor < 0 ? CreditRating.C :
                (int) currentRating + combinedFactor > 6 ? CreditRating.Aaa : currentRating + combinedFactor;
            return newRating;
        }
    }
}