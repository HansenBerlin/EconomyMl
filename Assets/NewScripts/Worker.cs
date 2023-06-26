using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts
{
    public class Worker
    {
        public decimal Money { get; set; } = 30;
        //public double Wage { get; set; } = 0;
        public int Health { get; set; } = 1000;
        public int UnemployedForMonth { get; set; } = 0;
        private const int MonthlyDemand = 100;
        private const int MonthlyMinimumDemand = MonthlyDemand / 2;
        private int _consumeInMonth;

        public List<InventoryItem> Inventory { get; set; } = new();
        //public int CompanyId { get; set; }
        private readonly System.Random _rand = new();
        
        //private Company _employedAtCompany = null;
        //public bool HasJob { get; private set; }
        public bool HasJob => _jobContract != null;

        private JobContract _jobContract = null;

        public Worker()
        {
            Inventory.Add(new InventoryItem
            {
                AvgPaid = 1, 
                Count = 0, 
                Product = ProductType.Food
            });
        }
        
        public void AddContract(JobContract contract)
        {
            _jobContract?.QuitContract();
            _jobContract = contract;
            UnemployedForMonth = 0;
        }

        public void Give(decimal sum)
        {
            Money += sum;
        }

        public void EndMonth()
        {
            Inventory[0].Count = Inventory[0].Count >= _consumeInMonth 
                ? Inventory[0].Count -= _consumeInMonth 
                : Inventory[0].Count = 0;
        }

        public void AddProductBids()
        {
            Academy.Instance.StatsRecorder.Add("Worker/Money", (float)Money);

            _consumeInMonth = MonthlyDemand + _rand.Next(MonthlyMinimumDemand * -1, MonthlyMinimumDemand + 1);
            decimal bidPrice = 0;
            if (Inventory[0].Count - _consumeInMonth < MonthlyMinimumDemand)
            {
                bidPrice = Inventory[0].AvgPaid * 1.1M;
            }
            else if (Inventory[0].Count - _consumeInMonth < MonthlyDemand)
            {
                bidPrice = Inventory[0].AvgPaid;
            }
            else if (Inventory[0].Count - _consumeInMonth < MonthlyDemand * 1.5)
            {
                bidPrice = Inventory[0].AvgPaid * 0.9M;
            }
            else
            {
                return;
            }

            bidPrice = bidPrice < 0.1M ? 0.1M : bidPrice > 5 ? 5 : bidPrice;
            var bpold = bidPrice;
            if (_consumeInMonth * bidPrice > Money)
            {
                bidPrice = RoundDown(Money / _consumeInMonth, 2);
            }
            if (_consumeInMonth < MonthlyMinimumDemand)
            {
                bidPrice = RoundDown(Money / MonthlyMinimumDemand, 2);
                _consumeInMonth = MonthlyMinimumDemand;
            }
            if (_consumeInMonth > 0 && bidPrice > 0)
            {
                if (Money - bidPrice * _consumeInMonth < 0)
                {
                    Debug.LogError("Not enough money");
                }
                var bid = new ProductBid(ProductType.Food, this, bidPrice, _consumeInMonth);
                ServiceLocator.Instance.ProductMarket.AddBid(bid);
                Academy.Instance.StatsRecorder.Add("Market/P-Bid-Make-Count", _consumeInMonth);
                Academy.Instance.StatsRecorder.Add("Market/P-Bid-Make-Price", (float)bidPrice);
            }
        }
        
        public decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }

        public void FullfillBid(ProductType product, int count, decimal price)
        {
            Inventory.Where(x => x.Product == product).ToArray()[0].Add(count, price);
            Money -= count * price;
        }
        
        public void RemoveJobContract(JobContract contract, bool isQuitByEmployer)
        {
            if (_jobContract != contract)
            {
                throw new Exception("Wrong contract: " + contract.Employer.Id);
            }

            if (isQuitByEmployer)
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

            decimal wage;
            if (HasJob)
            {
                if (_jobContract.Wage >= criticalBoundary)
                {
                    return;
                }

                wage = criticalBoundary;
                //wage = criticalBoundary * demandModifier;
            }
            else
            {
                decimal modifier = UnemployedForMonth < 2 ? 0.95M : UnemployedForMonth < 6 ? 0.85M : UnemployedForMonth < 12 ? 0.75M : 0.5M;
                wage = criticalBoundary * modifier;
                //wage = criticalBoundary * modifier * demandModifier;
            }
            
            //Debug.Log("WORKER Wage set to " + wage + " with " + UnemployedForMonth + " months unemployed");
            
            wage = wage < 40 ? 40 : wage > 150 ? 150 : wage;

            
            var offer = new JobOffer(this, wage);
            ServiceLocator.Instance.LaborMarket.AddJobOffer(offer);
            Academy.Instance.StatsRecorder.Add("Market/Job-Offer-Price", (float)wage);

        }
    }
}