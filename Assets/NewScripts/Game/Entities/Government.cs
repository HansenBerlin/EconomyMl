using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using NewScripts.Training;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class Government : Agent, IBidder
    {
        public List<GovernmentLedger> Ledgers { get; } = new();
        private decimal Liquidity { get; set; } = 100000;
        private int FoodSupply { get; set; }
        private List<ICompany> Companies => ServiceLocator.Instance.Companys;
        private EconomyMetricsCalculator _economyMetrics;
        private GlobalPolicies _policies;
        private RewardNormalizer _rewardNormalizer;
        private int _companyCount;
        private float _gdpAggregate;
        private float _inflationRateAggregate;
        private float _employmentRateAggregate;

        public void Init(GlobalPolicies policies, int companyCount, EconomyMetricsCalculator economyMetrics, 
            RewardNormalizer rewardNormalizer, decimal startingLiquidity)
        {
            Liquidity = startingLiquidity;
            _policies = policies;
            _companyCount = companyCount;
            _economyMetrics = economyMetrics;
            _rewardNormalizer = rewardNormalizer;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(FoodSupply);
            sensor.AddObservation(_policies.RichTaxRate);
            sensor.AddObservation((float)ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear());
            sensor.AddObservation(_employmentRateAggregate/ServiceLocator.Instance.FlowController.Month);
            sensor.AddObservation(_inflationRateAggregate/ServiceLocator.Instance.FlowController.Month);
            sensor.AddObservation(_gdpAggregate/ServiceLocator.Instance.FlowController.Month);
            sensor.AddObservation((float)Liquidity);
            sensor.AddObservation(_policies.CompanyTaxRate);
            sensor.AddObservation(_policies.SubsidyRate);
            sensor.AddObservation(_policies.FoodStamprate);
            sensor.AddObservation(_policies.SocialWelfareRate);
            sensor.AddObservation((float)_policies.MinimumWage);
        }

        //private bool commited = true;

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted = true;
            float taxRate = ValueMapper.MapValue(actionBuffers.ContinuousActions[0], 0, 0.9F);
            float minimumWage = ValueMapper.MapValue(actionBuffers.ContinuousActions[1], 1, 200F);
            (float subsidyRate, float socialWelfareRate, float foodStampRate) = 
                ValueMapper.MapPolicyActions(
                    actionBuffers.ContinuousActions[2], 
                    actionBuffers.ContinuousActions[3], 
                    actionBuffers.ContinuousActions[4]);
            float richtaxRate = ValueMapper.MapValue(actionBuffers.ContinuousActions[5], 0, 0.5F);
            _policies.CompanyTaxRate = taxRate;
            _policies.RichTaxRate = richtaxRate;
            _policies.MinimumWage = (decimal)minimumWage;
            _policies.SubsidyRate = subsidyRate;
            _policies.SocialWelfareRate = socialWelfareRate;
            _policies.FoodStamprate = foodStampRate;
        }

        public void EndYear()
        {
            ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted = false;

            _inflationRateAggregate /= 12;
            _gdpAggregate /= 12;
            _employmentRateAggregate /= 12;
            _rewardNormalizer.AddValue(_gdpAggregate);
           //Debug.Log($"YEAR Government: InflationRate: {_inflationRateAggregate:0.##}, " +
           //          $"Gdp: {_gdpAggregate:0}, EmploymentRate: {_employmentRateAggregate:0}, " +
           //          $"Liquidity: {Liquidity}, FoodSupply: {FoodSupply}");
            
            Academy.Instance.StatsRecorder.Add("GV/InflationRate", _inflationRateAggregate);
            Academy.Instance.StatsRecorder.Add("GV/Gdp", _gdpAggregate);
            Academy.Instance.StatsRecorder.Add("GV/EmploymentRate", _employmentRateAggregate);
            Academy.Instance.StatsRecorder.Add("GV/Liquidity", (float)Liquidity);
            Academy.Instance.StatsRecorder.Add("GV/FoodSupply", FoodSupply);
            Academy.Instance.StatsRecorder.Add("GV/CompanyTaxRate", _policies.CompanyTaxRate);
            Academy.Instance.StatsRecorder.Add("GV/RichTaxRate", _policies.RichTaxRate);
            Academy.Instance.StatsRecorder.Add("GV/SubsidyRate", _policies.SubsidyRate);
            Academy.Instance.StatsRecorder.Add("GV/FoodStamprate", _policies.FoodStamprate);
            Academy.Instance.StatsRecorder.Add("GV/SocialWelfareRate", _policies.SocialWelfareRate);
            Academy.Instance.StatsRecorder.Add("GV/MinimumWage", (float)_policies.MinimumWage);

            var ledger = new GovernmentLedger
            {
                InflationRate = _inflationRateAggregate,
                Gdp = _gdpAggregate,
                CompanyTaxRate = _policies.CompanyTaxRate,
                SubsidyRate = _policies.SubsidyRate,
                FoodStamprate = _policies.FoodStamprate,
                SocialWelfareRate = _policies.SocialWelfareRate,
                MinimumWage = _policies.MinimumWage,
                Liquidity = Liquidity,
                FoodSupply = FoodSupply,
                RichTaxRate = _policies.RichTaxRate
            };
            
            Ledgers.Add(ledger);

            var gdpReward = _rewardNormalizer.Normalize(_gdpAggregate, false);
            AddReward((float)gdpReward);
            _inflationRateAggregate = 0;
            _gdpAggregate = 0;
            _employmentRateAggregate = 0;

            if (ServiceLocator.Instance.FlowController.Year % 10 == 0)
            {
                _rewardNormalizer = new RewardNormalizer();
                EndEpisode();
            }
            var random = new System.Random();
            //float minimumWage = ValueMapper.MapValue(actionBuffers.ContinuousActions[1], 1, 200F);
            //(float subsidyRate, float socialWelfareRate, float foodStampRate) = ValueMapper.MapPolicyActions(
              //  random.Next(-100, 101) / 100F, random.Next(-100, 101) / 100F, random.Next(-100, 101) / 100F);
            //_policies.TaxRate = random.Next(40, 61) / 100F;
            //_policies.SubsidyRate = subsidyRate;
            //_policies.SocialWelfareRate = socialWelfareRate;
            //_policies.FoodStamprate = foodStampRate;
            
            //ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted = true;

        }

        public void AddFoodBids()
        {
            decimal bidPrice = ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear();
            int maxBuyAmount = (int)Math.Floor(Liquidity * (decimal)_policies.FoodStamprate / bidPrice);
            int maxNeeded = ServiceLocator.Instance.Settings.FoodDemandModifier * 50 * 1000 - FoodSupply;
            maxBuyAmount = maxNeeded < maxBuyAmount ? maxNeeded : maxBuyAmount;
            if (maxBuyAmount > 0)
            {
                var bid = new ProductBid(ProductType.Food, this, bidPrice, maxBuyAmount);
                ServiceLocator.Instance.FoodProductMarket.AddBid(bid);
            }
        }

        public void EndMonth()
        {
            _inflationRateAggregate += (float)_economyMetrics.CalculateInflationRate();
            _gdpAggregate += (float)_economyMetrics.CalculateBip(_companyCount) / 1000;
            _employmentRateAggregate += (float)ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates[^1]
                .OverallEmploymentRate;
            //Debug.Log($"MONTH Government: InflationRate: {_inflationRateAggregate}, Gdp: {_gdpAggregate}, EmploymentRate: {_employmentRateAggregate}");
            //PayOutSubsidy();
            //PayOutSocialFare();
            //DistributeFood();
        }
        

        public decimal PayCompanyTaxes(decimal profit)
        {
            var tax = profit * (decimal) _policies.CompanyTaxRate;
            Liquidity += tax;
            Academy.Instance.StatsRecorder.Add("GV/CompanyTaxIncome", (float)tax);
            return tax;
        }
        
        public decimal PayRichTaxes(decimal capital)
        {
            var tax = capital * (decimal) _policies.RichTaxRate;
            Liquidity += tax;
            Academy.Instance.StatsRecorder.Add("GV/RichTaxIncome", (float)tax);
            return tax;
        }

        public void PayOutSubsidy()
        {
            int companysCount = Companies.Count;
            var startups = Companies
                .Where(x => x.LifetimeMonths < 12 
                            || x.Liquidity < ServiceLocator.Instance.Settings.TotalMoneySupply / 10 / companysCount)
                .ToList();
            if (startups.Count == 0)
            {
                return;
            }

            decimal subsidityPerCompany = Liquidity * (decimal) _policies.SubsidyRate / startups.Count;
            foreach (var company in startups)
            {
                company.Liquidity += subsidityPerCompany;
                Liquidity -= subsidityPerCompany;
            }
            Academy.Instance.StatsRecorder.Add("GV/SubsidyPaid", (float)subsidityPerCompany * startups.Count);
        }
        
        public void FullfillBid(ProductType product, int count, decimal price)
        {
            if (count <= 0 || price <= 0)
            {
                throw new ArgumentException("Count and price must be positive");
            }
            Liquidity -= count * price;
            FoodSupply += count;
            Academy.Instance.StatsRecorder.Add("GV/Foodxpenses", count * (float)price);
        }

        public void PayOutSocialFare()
        {
            var unemployed =
                ServiceLocator.Instance.LaborMarket.Workers
                    .Where(x => x.HasJob == false && x.Money < ServiceLocator.Instance.Settings.TotalMoneySupply / 10000)
                    .ToList();

            if (unemployed.Count > 0)
            {
                decimal paymentPerHousehold = Liquidity * (decimal) _policies.SocialWelfareRate / unemployed.Count;
                foreach (var worker in unemployed)
                {
                    worker.PaySocialWelfare(paymentPerHousehold);
                    Liquidity -= paymentPerHousehold;
                }
                Academy.Instance.StatsRecorder.Add("GV/SocialWelfarePaid", (float)paymentPerHousehold * unemployed.Count);
            }
        }
        
        public void DistributeFood()
        {
            var hungry =
                ServiceLocator.Instance.LaborMarket.Workers.Where(
                    x => x.IsHungry).ToList();

            if (hungry.Count > 0)
            {
                int foodPerHousehold = (int)Math.Floor((float)FoodSupply / hungry.Count);
                if (foodPerHousehold == 0)
                {
                    return;
                }
                foreach (var worker in hungry)
                {
                    worker.GiveFood(foodPerHousehold);
                    FoodSupply -= foodPerHousehold;
                }
                Academy.Instance.StatsRecorder.Add("GV/FoodDistributed", foodPerHousehold * hungry.Count);
            }
        }
    }
}


