using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using UnityEngine;

namespace NewScripts.Training
{
    public class ReputationAggregator
    {
        public float Reputation => CalculateReputation();

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
            _profitNormalizer.AddValue(_rewardValueModel.Profit);
            _lifetimeNormalizer.AddValue(_rewardValueModel.Lifetime);
            _workerContractRuntimeNormalizer.AddValue(_rewardValueModel.WorkerContractRuntime);
            _foodMarketshareNormalizer.AddValue(_rewardValueModel.FoodSales);
            _luxuryMarketshareNormalizer.AddValue(_rewardValueModel.LuxurySales);
        }

        public void AddProfitChange()
        {
            _profitReputation = _profitNormalizer.Normalize(_rewardValueModel.Profit, true);
        }
        
        public void AddLifetimeChange()
        {
            _lifetimeReputation = _lifetimeNormalizer.Normalize(_rewardValueModel.Lifetime, true);
        }
        
        public void AddWorkerContractRuntimeChange()
        {
            _workerContractRuntimeReputation = _workerContractRuntimeNormalizer.Normalize(_rewardValueModel.WorkerContractRuntime, true);
        }
        
        public void AddMarketShareChange()
        {
            //int totalSalesFood = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates[^1].AverageSalesFood;
            //int totalSalesLuxury = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates[^1].AverageSalesLuxury;
            var foodReputation = _foodMarketshareNormalizer.Normalize(_rewardValueModel.FoodSales, true);
            var luxuryReputation = _luxuryMarketshareNormalizer.Normalize(_rewardValueModel.LuxurySales, true);
            _marketShareReputation = (foodReputation + luxuryReputation) / 2;
        }
        
        private float CalculateReputation()
        {
            var reputation = (float) ((_profitReputation + _workerContractRuntimeReputation + _marketShareReputation) / 3);
            //Debug.Log($"Total: {reputation*100:0.##} Profit: {_profitReputation*100:0.##}, WorkerContractRuntime: {_workerContractRuntimeReputation*100:0.##}, MarketShare: {_marketShareReputation*100:0.##}");
            return reputation;
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