using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NewScripts.Game.Models;
using NewScripts.Game.Services;

namespace NewScripts.Training
{
    public class ReputationAggregator
    {
        public float Reputation => (float) ((_profitReputation + _lifetimeReputation + _workerContractRuntimeReputation + _marketShareReputation) / 4);

        private readonly RewardNormalizer _profitNormalizer;
        private readonly RewardNormalizer _lifetimeNormalizer;
        private readonly RewardNormalizer _workerContractRuntimeNormalizer;
        private readonly RewardNormalizer _foodMarketshareNormalizer;
        private readonly RewardNormalizer _luxuryMarketshareNormalizer;

        private double _marketShareReputation;
        private double _profitReputation;
        private double _lifetimeReputation;
        private double _workerContractRuntimeReputation;
        
        private RewardValueModel _rewardValueModel;
        
        public ReputationAggregator(List<RewardNormalizer> normalizers)
        {
            if(normalizers.Count != 5)
            {
                throw new System.Exception("ReputationAggregator needs 5 normalizers");
            }
            _profitNormalizer = normalizers[0];
            _lifetimeNormalizer = normalizers[1];
            _workerContractRuntimeNormalizer = normalizers[2];
            _foodMarketshareNormalizer = normalizers[3];
            _luxuryMarketshareNormalizer = normalizers[4];
        }

        public void AddValuesToNormalizers(double profit, double lifetime, List<JobContract> contracts, int foodSales, int luxurySales)
        {
            _rewardValueModel = new RewardValueModel
            {
                Profit = profit,
                Lifetime = lifetime,
                WorkerContractRuntime = contracts.Sum(contract => contract.ShortWorkForMonths > 0 ? (double) contract.RunsFor / 2 : contract.RunsFor),
                FoodSales = foodSales,
                LuxurySales = luxurySales
            };
        }

        public void AddProfitChange()
        {
            _profitReputation = _profitNormalizer.Normalize(_rewardValueModel.Profit);
        }
        
        public void AddLifetimeChange()
        {
            _lifetimeReputation = _lifetimeNormalizer.Normalize(_rewardValueModel.Lifetime);
        }
        
        public void AddWorkerContractRuntimeChange()
        {
            _workerContractRuntimeReputation = _workerContractRuntimeNormalizer.Normalize(_rewardValueModel.WorkerContractRuntime);
        }
        
        public void AddMarketShareChange()
        {
            int totalSalesFood = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates[^1].AverageSalesFood;
            int totalSalesLuxury = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates[^1].AverageSalesLuxury;
            var foodReputation = _foodMarketshareNormalizer.Normalize(_rewardValueModel.FoodSales / (double)totalSalesFood);
            var luxuryReputation = _luxuryMarketshareNormalizer.Normalize(_rewardValueModel.LuxurySales / (double)totalSalesLuxury);
            _marketShareReputation = (foodReputation + luxuryReputation) / 2;
        }
    }

    public class RewardValueModel
    {
        public double Profit { get; set; }
        public double Lifetime { get; set; }
        public double WorkerContractRuntime { get; set; }
        public int FoodSales { get; set; }
        public int LuxurySales { get; set; }
    }
}