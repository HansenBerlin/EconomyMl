using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class Worker
    {
        public decimal Money { get; set; } = 500;
        public bool HasJob => _jobContract != null;
        
        private int UnemployedForMonth { get; set; }
        private readonly List<InventoryItem> _inventory = new();
        private HouseholdData _periodData = new();
        private JobContract _jobContract;
        private readonly BidCalculatorService _bidCalculatorService;

        public Worker(BidCalculatorService bidCalculatorService)
        {
            _bidCalculatorService = bidCalculatorService;
            var food = new InventoryItem(ProductType.Food, 150, 50, 250, 1);
            var luxury = new InventoryItem(ProductType.Luxury, 10, 0, 100, 10);
            _inventory.Add(food);
            _inventory.Add(luxury);
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
            foreach (var item in _inventory)
            {
                item.Consume();
            }
            _periodData.JobStatus = _jobContract == null 
                ? WorkerJobStatus.Unemployed 
                : _jobContract.ShortWorkForMonths > 0 
                    ? WorkerJobStatus.ShortTimeWork 
                    : WorkerJobStatus.FullyEmployed;
            _periodData.RealWage = _jobContract?.Wage ?? 0;
            ServiceLocator.Instance.HouseholdAggregator.Add(_periodData);
            _periodData = new HouseholdData();
        }

        

        public void AddProductBids(decimal averagePrice, ProductType productType, ProductMarket market)
        {
            var inventoryItem = _inventory.FirstOrDefault(x => x.Product == productType)??
                                throw new Exception("No inventory item for " + productType);
            
            Academy.Instance.StatsRecorder.Add("New/Money-Before-Bid " + productType, (float)Money);
            (int low, int high) = _bidCalculatorService.DemandModifier(inventoryItem);
            inventoryItem.ConsumeInMonth = _bidCalculatorService.DemandByMarginalUtility(low, high);
            decimal bidPrice = _bidCalculatorService.DetermineBiddingPrice(averagePrice, inventoryItem);
            
            if (inventoryItem.ConsumeInMonth * bidPrice > Money)
            {
                inventoryItem.ConsumeInMonth = (int)Math.Floor(Money / bidPrice * 0.9M);
            }
            if (inventoryItem.ConsumeInMonth > 0 && bidPrice > 0)
            {
                if (productType == ProductType.Luxury)
                {
                    var food = _inventory.FirstOrDefault(x => x.Product == ProductType.Food);
                    var fullfillRatioBaseDemand = (food!.FullfilledInMonth + 1) / ((float)food.ConsumeInMonth + 1);
                    inventoryItem.ConsumeInMonth = (int)Math.Floor(fullfillRatioBaseDemand * inventoryItem.ConsumeInMonth);
                }
                if (Money - bidPrice * inventoryItem.ConsumeInMonth < 0)
                {
                    throw new Exception("Not enough money");
                }
                var bid = new ProductBid(productType, this, bidPrice, inventoryItem.ConsumeInMonth);
                market.AddBid(bid);
                
                Academy.Instance.StatsRecorder.Add("Market/BidCount " + productType, inventoryItem.ConsumeInMonth);
                Academy.Instance.StatsRecorder.Add("Market/BidPrice " + productType, (float)bidPrice);
            }
            if (inventoryItem.ConsumeInMonth < 0 || inventoryItem.ConsumeInMonth > 1000 || double.IsInfinity(inventoryItem.ConsumeInMonth) ||
                double.IsNaN(inventoryItem.ConsumeInMonth))
            {
                throw  new Exception("Wrong demand: " + inventoryItem.ConsumeInMonth);
            }
            
            AddData(inventoryItem, bidPrice);
        }

        private void AddData(InventoryItem inventoryItem, decimal bidPrice)
        {
            _periodData.MoneyAvailableAdBidTime =  Money;
            if (inventoryItem.Product == ProductType.Food)
            {
                _periodData.DemandFood = inventoryItem.ConsumeInMonth;
                _periodData.FoodInventoryBeforeBuying = inventoryItem.Count;
                _periodData.PriceBidFood =  bidPrice;
            }
            else
            {
                _periodData.DemandLuxury = inventoryItem.ConsumeInMonth;
                _periodData.LuxuryInventoryBeforeBuying = inventoryItem.Count;
                _periodData.PriceBidLuxury =  bidPrice;
            }
        }

        public void FullfillBid(ProductType product, int count, decimal price)
        {
            var item = _inventory.FirstOrDefault(x => x.Product == product);
            item?.Add(count, price);
            Money -= count * price;
            Academy.Instance.StatsRecorder.Add("New/Money-After-Buy " + product, (float)Money);
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
            Academy.Instance.StatsRecorder.Add("New/Money-Before-Job-Search", (float)Money);

            //Academy.Instance.StatsRecorder.Add("Worker/Bought", DemandFulfilled);
            if (HasJob)
            {
                _jobContract.RunsFor++;
                if (_jobContract.RunsFor <= 3 && _jobContract.ShortWorkForMonths == 0)
                {
                    _periodData.ReservationWage = _jobContract.Wage;
                    return;
                }
            }
            UnemployedForMonth = HasJob ? 0 : UnemployedForMonth + 1;

            averageFoodPrice *= _inventory.FirstOrDefault(x => x.Product == ProductType.Food)!.MonthlyMinimumDemand;
            double demand = ServiceLocator.Instance.LaborMarket.DemandForWorkforce;
            double demandModifier = (demand + 1001) / 1000;
            decimal criticalBoundary = averageIncome * 0.75M > averageFoodPrice ? averageIncome + 0.75M : averageFoodPrice;

            decimal requiredWage;
            if (HasJob)
            {
                decimal currentWage = _jobContract.ShortWorkForMonths >= 3 ? _jobContract.Wage / 2 : _jobContract.Wage;
                if (currentWage >= averageFoodPrice && currentWage >= averageIncome * 0.75M)
                {
                    _periodData.ReservationWage = currentWage;
                    return;
                }

                if(_jobContract.ShortWorkForMonths == 0)
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
            _periodData.ReservationWage = requiredWage;
        }
    }
}