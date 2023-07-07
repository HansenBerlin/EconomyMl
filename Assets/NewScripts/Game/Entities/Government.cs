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

namespace NewScripts.Game.Entities
{
    public class Government : Agent, IBidder
    {
        public List<GovernmentLedger> Ledgers { get; } = new();
        private decimal Liquidity { get; set; }
        private int FoodSupply { get; set; }
        private List<ICompany> Companies => ServiceLocator.Instance.Companys;
        private EconomyMetricsCalculator _economyMetrics;
        private GlobalPolicies _policies;
        private RewardNormalizer _rewardNormalizer;
        private int _companyCount;
        private float _gdpAggregate;
        private float _inflationRateAggregate;
        private float _employmentRateAggregate;

        public void Init(GlobalPolicies policies, int companyCount, EconomyMetricsCalculator economyMetrics, RewardNormalizer rewardNormalizer)
        {
            _policies = policies;
            _companyCount = companyCount;
            _economyMetrics = economyMetrics;
            _rewardNormalizer = rewardNormalizer;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_employmentRateAggregate);
            sensor.AddObservation(_inflationRateAggregate);
            sensor.AddObservation(_gdpAggregate);
            sensor.AddObservation(FoodSupply);
            sensor.AddObservation((float)Liquidity);
            sensor.AddObservation(_policies.TaxRate);
            sensor.AddObservation(_policies.SubsidyRate);
            sensor.AddObservation(_policies.FoodStamprate);
            sensor.AddObservation(_policies.SocialWelfareRate);
            sensor.AddObservation((float)_policies.MinimumWage);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            float taxRate = ValueMapper.MapValue(actionBuffers.ContinuousActions[0], 0, 0.9F);
            float minimumWage = ValueMapper.MapValue(actionBuffers.ContinuousActions[1], 1, 200F);
            (float subsidyRate, float socialWelfareRate, float foodStampRate) = ValueMapper.MapPolicyActions(actionBuffers.ContinuousActions[2], 
                actionBuffers.ContinuousActions[3], actionBuffers.ContinuousActions[4]);
            _policies.TaxRate = taxRate;
            _policies.MinimumWage = (decimal)minimumWage;
            _policies.SubsidyRate = subsidyRate;
            _policies.SocialWelfareRate = socialWelfareRate;
            _policies.FoodStamprate = foodStampRate;
            _inflationRateAggregate = 0;
            _gdpAggregate = 0;
            ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted = true;
        }

        public void EndYear()
        {
            
            ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted = false;
            _inflationRateAggregate /= 12;
            _gdpAggregate /= 12;
            _employmentRateAggregate /= 12;
            _rewardNormalizer.AddValue(_gdpAggregate);
            
            var ledger = new GovernmentLedger
            {
                InflationRate = _inflationRateAggregate,
                Gdp = _gdpAggregate,
                TaxRate = _policies.TaxRate,
                SubsidyRate = _policies.SubsidyRate,
                FoodStamprate = _policies.FoodStamprate,
                SocialWelfareRate = _policies.SocialWelfareRate,
                MinimumWage = _policies.MinimumWage,
                Liquidity = Liquidity,
                FoodSupply = FoodSupply
            };
            
            Ledgers.Add(ledger);

            var gdpReward = _rewardNormalizer.Normalize(_gdpAggregate);
            AddReward((float)gdpReward);

            if (Liquidity < 10000)
            {
                SetReward(-100f);
                EndEpisode();
            }
            RequestDecision();
        }

        public void AddFoodBids()
        {
            decimal bidPrice = ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear();
            int foodAmount = (int)Math.Floor(Liquidity * (decimal)_policies.FoodStamprate / bidPrice);
            var bid = new ProductBid(ProductType.Food, this, bidPrice, foodAmount);
            ServiceLocator.Instance.FoodProductMarket.AddBid(bid);
        }

        public void EndMonth()
        {
            _inflationRateAggregate += (float)_economyMetrics.CalculateInflationRate();
            _gdpAggregate += (float)_economyMetrics.CalculateBip(_companyCount) / 1000;
            _employmentRateAggregate += (float)ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates[^1]
                .OverallEmploymentRate;
            //PayOutSubsidy();
            //PayOutSocialFare();
            //DistributeFood();
        }
        

        public decimal PayTaxes(decimal profit)
        {
            var tax = profit * (decimal) _policies.TaxRate;
            Liquidity += tax;
            return tax;
        }

        public void PayOutSubsidy()
        {
            var startups = Companies.Where(x => x.LifetimeMonths < 12).ToList();
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
        }
        
        public void FullfillBid(ProductType product, int count, decimal price)
        {
            Liquidity -= count * price;
            FoodSupply += count;
        }

        public void PayOutSocialFare()
        {
            var unemployed =
                ServiceLocator.Instance.LaborMarket.Workers.Where(
                    x => x.HasJob == false).ToList();

            if (unemployed.Count > 0)
            {
                decimal paymentPerHousehold = Liquidity * (decimal) _policies.SocialWelfareRate / unemployed.Count;
                foreach (var worker in unemployed)
                {
                    worker.PaySocialWelfare(paymentPerHousehold);
                }

                Liquidity -= paymentPerHousehold;
            }
        }
        
        public void DistributeFood()
        {
            var hungry =
                ServiceLocator.Instance.LaborMarket.Workers.Where(
                    x => x.IsHungry == false).ToList();
            decimal averageMarketPrice = ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear();

            if (hungry.Count > 0)
            {
                int foodPerHousehold = (int)Math.Floor((float)FoodSupply / hungry.Count);
                foreach (var worker in hungry)
                {
                    worker.GiveFood(foodPerHousehold, averageMarketPrice);
                    FoodSupply -= foodPerHousehold;
                }
            }
        }
    }
}


