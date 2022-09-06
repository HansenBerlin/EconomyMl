using Assets.Scripts.Enums;

namespace Assets.Scripts.Controller
{
    public static class RatingController
    {
        public static CreditRating Calculate(decimal balance, decimal profitTrend, decimal loansTakenSum, decimal lastMonthProfit, CreditRating currentRating)
        {
            if (loansTakenSum > balance)
            {
                //return CreditRating.C;
            }
            var balanceFactor = balance < -100000 ? -2 : balance <= 0 ? -1 : balance > 1000000 ? +2 : 1;
            var profitFactor = profitTrend < -0.5M ? -2 : profitTrend <= 0 ? -1 : profitTrend > 0.5M ? 2 : 1;
            var monthlyAvailableCapital = lastMonthProfit - loansTakenSum / 12;
            var loansFactor = monthlyAvailableCapital > 0 ? 1 : -1;
            var combinedFactor = balanceFactor + profitFactor + loansFactor;
            var newRating = (int) currentRating + combinedFactor < 0 ? CreditRating.C :
                (int) currentRating + combinedFactor > 6 ? CreditRating.AAA : currentRating + combinedFactor;
            return newRating;
        }
    }
}