using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts
{
    public class Worker
    {
        public decimal Money { get; set; } = 90;
        public int Health { get; set; } = 1000;
        public int ConsumeInMonth { get; private set; }
        private int FullfilledInMonth { get; set; }
        public bool HasJob => _jobContract != null;
        //private const int MonthlyDemand = 100;
        private const int MonthlyMaximumDemand = 250;
        private const int MonthlyMinimumDemand = 50;
        private const int MonthlyAverageDemand = 100;
        private int UnemployedForMonth { get; set; } = 0;
        private readonly List<InventoryItem> _inventory = new();
        private readonly System.Random _rand = new();

        private JobContract _jobContract;

        public Worker()
        {
            _inventory.Add(new InventoryItem
            {
                AvgPaid = 1, 
                Count = MonthlyMinimumDemand, 
                Product = ProductType.Food
            });
        }
        
        public void AddContract(JobContract contract)
        {
            _jobContract?.QuitContract(WorkerFireReason.WorkerDecision);
            _jobContract = contract;
            UnemployedForMonth = 0;
        }

        public void Give(decimal sum)
        {
            Money += sum;
        }

        public void EndMonth()
        {
            _inventory[0].Consume(ConsumeInMonth);
        }

        private (int low, int high) DemandModifier()
        {
            float ratio = FullfilledInMonth > 0 && ConsumeInMonth > 0 ? FullfilledInMonth / (float)ConsumeInMonth : 0;
            float modifier = (ratio + 1) / 2;
            int low = (int)(MonthlyMinimumDemand * modifier);
            int high = (int)(MonthlyMaximumDemand * modifier);
            FullfilledInMonth = 0;
            return (low, high);
        }

        private decimal DetermineBiddingPrice(decimal averageMarketPrice)
        {
            decimal priceWillingness = ConsumeInMonth - _inventory[0].Count;

            if (_inventory[0].Count < MonthlyMinimumDemand)
            {
                priceWillingness += _inventory[0].AvgPaid - averageMarketPrice; 
            }

            decimal minPrice = averageMarketPrice / 2;
            decimal maxPrice = averageMarketPrice * 2;
            //decimal biddingPrice = Math.Max(minPrice, Math.Min(maxPrice, _inventory[0].AvgPaid + priceWillingness / (MonthlyDemand * 2 - MonthlyMinimumDemand)));
            decimal biddingPrice = Math.Max(minPrice, Math.Min(maxPrice, _inventory[0].AvgPaid + priceWillingness / (MonthlyAverageDemand * 2 - MonthlyMinimumDemand)));

            return biddingPrice;
        }
        
        private int DemandByMarginalUtility(int minDemand, int maxDemand)
        {
            double random = _rand.NextDouble();
            int demand = minDemand + (int)Math.Round(Math.Pow(random, 2) * (maxDemand - minDemand));
            return demand;
        }

        public void AddProductBids(decimal averagePrice)
        {
            Academy.Instance.StatsRecorder.Add("Worker/Money", (float)Money);
            (int low, int high) = DemandModifier();
            ConsumeInMonth = DemandByMarginalUtility(low, high);
            decimal bidPrice = DetermineBiddingPrice(averagePrice);
            
            if (ConsumeInMonth * bidPrice > Money)
            {
                ConsumeInMonth = (int)Math.Floor(Money / bidPrice * 0.9M);
            }
            if (ConsumeInMonth > 0 && bidPrice > 0)
            {
                BidPrice = bidPrice;
                if (Money - bidPrice * ConsumeInMonth < 0)
                {
                    throw new Exception("Not enough money");
                }
                var bid = new ProductBid(ProductType.Food, this, bidPrice, ConsumeInMonth);
                ServiceLocator.Instance.ProductMarket.AddBid(bid);
                Academy.Instance.StatsRecorder.Add("Market/P-Bid-Make-Count", ConsumeInMonth);
                Academy.Instance.StatsRecorder.Add("Market/P-Bid-Make-Price", (float)bidPrice);
            }
            if (ConsumeInMonth < 0 || ConsumeInMonth > 1000 || double.IsFinite(ConsumeInMonth) ||
                double.IsNaN(ConsumeInMonth))
            {
                Debug.Log(ConsumeInMonth);
            }
            ServiceLocator.Instance.HouseholdAggregator.Add(new HouseholdLedger(HasJob, ConsumeInMonth, Money));

        }

        public decimal BidPrice;
        
        public decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }

        public void FullfillBid(ProductType product, int count, decimal price)
        {
            _inventory.Where(x => x.Product == product).ToArray()[0].Add(count, price);
            Money -= count * price;
            FullfilledInMonth += count;
        }
        
        public void RemoveJobContract(JobContract contract, WorkerFireReason reason)
        {
            if (_jobContract != contract)
            {
                throw new Exception("Wrong contract: " + contract.Employer.Id);
            }

            if (reason != WorkerFireReason.WorkerDecision)
            {
                // blacklist
            }
            _jobContract = null;
        }

        

        public void SearchForJob(decimal averageIncome, decimal averageFoodPrice)
        {
            //Academy.Instance.StatsRecorder.Add("Worker/Bought", DemandFulfilled);
            if (HasJob)
            {
                _jobContract.RunsFor++;
                if (_jobContract.RunsFor <= 3 && _jobContract.IsForceReduced == false)
                {
                    return;
                }
            }
            UnemployedForMonth = HasJob ? 0 : UnemployedForMonth + 1;

            averageFoodPrice *= MonthlyMinimumDemand;
            decimal criticalBoundary = averageIncome > averageFoodPrice ? averageIncome : averageFoodPrice;
            double demand = ServiceLocator.Instance.LaborMarket.DemandForWorkforce;
            double demandModifier = (demand + 1001) / 1000;

            decimal requiredWage;
            if (HasJob)
            {
                decimal currentWage = _jobContract.IsForceReduced ? _jobContract.Wage / 2 : _jobContract.Wage;
                if (currentWage >= criticalBoundary)
                {
                    return;
                }

                if(_jobContract.IsForceReduced == false)
                {
                    requiredWage = currentWage > criticalBoundary ? currentWage * 1.1M : criticalBoundary;
                }
                else
                {
                    requiredWage = currentWage > criticalBoundary ? currentWage * 0.9M : criticalBoundary;
                }
            }
            else
            {
                decimal modifier = UnemployedForMonth < 2 ? 0.95M : UnemployedForMonth < 6 ? 0.85M : UnemployedForMonth < 12 ? 0.75M : 0.5M;
                requiredWage = criticalBoundary * modifier;
                //wage = criticalBoundary * modifier * demandModifier;
            }
            
            //Debug.Log("WORKER Wage set to " + wage + " with " + UnemployedForMonth + " months unemployed");
            
            requiredWage = requiredWage < 25 ? 25 : requiredWage > 500 ? 500 : requiredWage;

            
            var offer = new JobOffer(this, requiredWage);
            ServiceLocator.Instance.LaborMarket.AddJobOffer(offer);
            Academy.Instance.StatsRecorder.Add("Market/Job-Offer-Price", (float)requiredWage);

        }
    }
}