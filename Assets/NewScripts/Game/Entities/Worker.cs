using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class Worker : IBidder
    {
        public decimal Money { get; set; }
        public bool HasJob => _jobContract != null;
        public bool IsHungry => IsMinimumFoodDemandFullfilledHungry();
        
        private int UnemployedForMonth { get; set; }
        private readonly List<InventoryItem> _inventory = new();
        private HouseholdData _periodData = new();
        private JobContract _jobContract;
        private readonly BidCalculatorService _bidCalculatorService;
        private Models.Settings _settings;

        public Worker(BidCalculatorService bidCalculatorService, decimal startingMoney, Models.Settings settings)
        {
            _settings = settings;
            Money = startingMoney;
            _bidCalculatorService = bidCalculatorService;
            var food = new InventoryItem(ProductType.Food, 150, 50, 250, 
                (settings.LowerPriceBoundaryFood + settings.UpperPriceBoundaryFood) / 2);
            var luxury = new InventoryItem(ProductType.Luxury, 10, 0, 100, 
                (settings.LowerPriceBoundaryLuxury + settings.UpperPriceBoundaryLuxury) / 2);
            _inventory.Add(food);
            _inventory.Add(luxury);
        }
        
        public void AddContract(JobContract contract)
        {
            _jobContract?.QuitContract(WorkerFireReason.WorkerDecision);
            _jobContract = contract;
            UnemployedForMonth = 0;
        }

        public void PaySocialWelfare(decimal sum)
        {
            Money += sum;
        }

        public void GiveFood(int count)
        {
            _inventory.First(x => x.Product == ProductType.Food).Add(count, 0, true);
        }

        public void Consume(ProductType type)
        {
            foreach (var item in _inventory.Where(x => x.Product == type))
            {
                item.Consume();
            }
        }

        public void EndMonth()
        {
            foreach (var item in _inventory)
            {
                item.FullfilledInMonth = 0;
            }
            _periodData.JobStatus = _jobContract == null 
                ? WorkerJobStatus.Unemployed 
                : _jobContract.ShortWorkForMonths > 0 
                    ? WorkerJobStatus.ShortTimeWork 
                    : WorkerJobStatus.FullyEmployed;
            _periodData.RealWage = _jobContract?.Wage ?? 0;
            decimal richTax = 0;
            if (Money > ServiceLocator.Instance.Settings.TotalMoneySupply / 2000)
            {
                richTax = ServiceLocator.Instance.Government.PayRichTaxes(Money);
                Money -= richTax;
            }
            _periodData.RichTaxPaid = richTax;
            ServiceLocator.Instance.HouseholdAggregator.Add(_periodData);
            _periodData = new HouseholdData();
            
        }

        

        public void AddProductBids(decimal averagePrice, ProductType productType, ProductMarket market)
        {
            var inventoryItem = _inventory.FirstOrDefault(x => x.Product == productType)??
                                throw new Exception("No inventory item for " + productType);
            
            //Academy.Instance.StatsRecorder.Add("New/Money-Before-Bid " + productType, (float)Money);
            (int low, int high) = _bidCalculatorService.DemandModifier(inventoryItem);
            inventoryItem.ConsumeInMonth = _bidCalculatorService.DemandByMarginalUtility(low, high);
            decimal bidPrice = _bidCalculatorService.DetermineBiddingPrice(averagePrice, inventoryItem);
            
            if (inventoryItem.ConsumeInMonth * bidPrice > Money)
            {
                inventoryItem.ConsumeInMonth = (int)Math.Floor(Money / bidPrice * 0.98M);
            }
            if (productType == ProductType.Luxury)
            {
                var food = _inventory.FirstOrDefault(x => x.Product == ProductType.Food);
                var fullfillRatioBaseDemand = (food!.FullfilledInMonth + 1) / ((float)food.ConsumeInMonth + 1);
                inventoryItem.ConsumeInMonth = (int)Math.Floor(fullfillRatioBaseDemand * inventoryItem.ConsumeInMonth);
            }
            if (inventoryItem.ConsumeInMonth > 0 && bidPrice > 0)
            {
                var bid = new ProductBid(productType, this, bidPrice, inventoryItem.ConsumeInMonth);
                market.AddBid(bid);
            }

            AddData(inventoryItem, bidPrice);
        }

        private bool IsMinimumFoodDemandFullfilledHungry()
        {
            var foodInventory = _inventory.FirstOrDefault(x => x.Product == ProductType.Food)!;
            return foodInventory.Count < foodInventory.MonthlyMinimumDemand;
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
            item?.Add(count, price, false);
            Money -= count * price;
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

            averageFoodPrice *= _inventory.FirstOrDefault(x => x.Product == ProductType.Food)!.MonthlyAverageDemand;
            decimal criticalBoundary = averageFoodPrice;

            decimal requiredWage;
            if (HasJob)
            {
                decimal currentWage = _jobContract.ShortWorkForMonths >= 3 ? _jobContract.Wage / 2 : _jobContract.Wage;
                if (currentWage >= averageFoodPrice && _jobContract.ShortWorkForMonths < 3)
                {
                    _periodData.ReservationWage = currentWage;
                    return;
                }

                requiredWage = currentWage > criticalBoundary ? currentWage * 0.9M : criticalBoundary;
            }
            else
            {
                decimal modifier = UnemployedForMonth < 2 ? 0.95M 
                    : UnemployedForMonth < 6 ? 0.85M 
                    : UnemployedForMonth < 12 ? 0.75M 
                    : UnemployedForMonth < 24 ? 0.5M 
                    : UnemployedForMonth < 26 ? 0.3M 
                    : 0.1M;
                requiredWage = criticalBoundary * modifier;
                //wage = criticalBoundary * modifier * demandModifier;
            }
            
            //Debug.Log("WORKER Wage set to " + wage + " with " + UnemployedForMonth + " months unemployed");
            
            requiredWage = requiredWage < _settings.LowerWageBoundary 
                ? _settings.LowerWageBoundary 
                : requiredWage > _settings.UpperWageBoundary 
                    ? _settings.UpperWageBoundary 
                    : requiredWage;

            
            var offer = new JobOffer(this, requiredWage);
            ServiceLocator.Instance.LaborMarket.AddJobOffer(offer);
            Academy.Instance.StatsRecorder.Add("Market/Job-Offer-Price", (float)requiredWage);
            _periodData.ReservationWage = requiredWage;
        }
    }
}