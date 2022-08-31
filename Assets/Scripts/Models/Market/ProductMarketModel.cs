using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Enums;
using Models.Production;
using Repositories;

namespace Models.Market
{



    public class ProductMarketModel
    {
        public string Id = Guid.NewGuid().ToString();

        private readonly List<ProductController> _productsAvailable = new();
        public readonly ProductType Type;
        private readonly IProductionTemplate _template;

        private readonly ProductMarketDataRepository _dataRepository;

        //public long LastUnfulfilledDemand { get; set; }
        //public long CurrentUnfulfilledDemand { get; private set; }
        //public long AverageDemandLastAndThisMonth => GetTotalDemand();
        public decimal AveragePrice { get; private set; }
        public long TotalSupply => Supply();

        private long _currentSales;
        private long _currentUnfullfilledDemand;
        private long _currentProduction;


        public decimal ObserveInflationRate = 0;

        private decimal _defaultPrice;
        //public double DemandChange => CurrentUnfulfilledDemand - LastUnfulfilledDemand;


        public ProductMarketModel(ProductType type, ProductMarketDataRepository dataRepository,
            IProductionTemplate template)
        {
            Type = type;
            _dataRepository = dataRepository;
            _template = template;
            //Reset();
        }

        public void AddProduct(ProductController product)
        {
            if (_defaultPrice == 0)
                _defaultPrice = product.Price;
            _productsAvailable.Add(product);
            UpdateProperties();
        }

        public decimal GetMarketShare(string productId)
        {
            var product = _productsAvailable.FirstOrDefault(p => p.Id == productId);
            if (_productsAvailable.Count == 1 && product != null)
            {
                return 1;
            }

            //decimal marketShare = _totalBought != 0 ? product.SalesAverageAllTime / _totalBought : 0;
            // TODO wieder rein?
            return 0;
        }

        public decimal GetLastQuarterDemandTrend()
        {
            var data = _dataRepository.Demand;
            double productionForMinimalCompany = (double) _template.UnitsPerWorker * 10;
            double meanTrend = StatisticalDistributionController.MeanTrend(data);
            if (productionForMinimalCompany < meanTrend)
            {
                return 0;
            }

            decimal trend = StatisticalDistributionController.NormalizedTrend(data, _currentUnfullfilledDemand);
            return trend;
        }

        /*private long GetTotalDemand()
        {
            long total = 0;
            foreach (var p in ProductsAvailable)
            {
                total += p.LastMonthUnfullfilledDemand + p.CurrentMonthUnfullfilledDemand;
            }
    
            return total / 2;
        }*/

        public ReceiptModel BuyMaxProductsForMoney(ProductRequestModel buyRequest)
        {
            decimal moneyAmount = buyRequest.TotalSpendable;
            var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();
            ReceiptModel receipt = new ReceiptModel();
            foreach (var p in matchingProducts)
            {
                var tempReciept = p.BuyFor(moneyAmount);
                decimal moneyPaid = tempReciept.TotalPricePaid;
                receipt.AmountBought += tempReciept.AmountBought;
                receipt.TotalPricePaid += moneyPaid;
                moneyAmount -= moneyPaid;
                if (moneyAmount == 0)
                {
                    break;
                }
            }

            _currentSales += receipt.AmountBought;

            return receipt;

        }

        public ReceiptModel BuyMaxProductsForMaxAmountSpended(ProductRequestModel buyRequest)
        {
            decimal moneyAmount = buyRequest.TotalSpendable;
            int maxAmount = buyRequest.MaxAmount;
            int demandAlreadyRequestedDueTooHighPrice = 0;
            List<ProductController> productsToRequestMissingSupply = new();
            var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();
            ReceiptModel receipt = new ReceiptModel();
            foreach (var p in matchingProducts)
            {
                var tempReciept = p.BuyFor(moneyAmount, maxAmount);
                decimal moneyPaid = tempReciept.TotalPricePaid;
                int amountBought = tempReciept.AmountBought;
                receipt.AmountBought += tempReciept.AmountBought;
                receipt.TotalPricePaid += moneyPaid;
                maxAmount -= amountBought;
                moneyAmount -= moneyPaid;

                /*if (tempReciept.BuyRejectType == BuyRejectType.MissingSupply)
                {
                    productsToRequestMissingSupply.Add(p);
                }
                else if(tempReciept.BuyRejectType == BuyRejectType.TooExpensive)
                {
                    p.RequestDemandMissing(maxAmount, buyRequest.DayOfMonthDivisor, BuyRejectType.TooExpensive);
                    demandAlreadyRequestedDueTooHighPrice += maxAmount;
                }*/

                if (moneyAmount == 0 || maxAmount == 0)
                {
                    break;
                }

            }

            /*int unrequestedDemandLeft = maxAmount - demandAlreadyRequestedDueTooHighPrice;
    
            if (unrequestedDemandLeft > 0)
            {
                int productCount = productsToRequestMissingSupply.Count;
    
                foreach (var pr in productsToRequestMissingSupply)
                {
                    pr.RequestDemandMissing(unrequestedDemandLeft / productCount, buyRequest.DayOfMonthDivisor, BuyRejectType.MissingSupply);
                }
            }*/

            _currentSales += receipt.AmountBought;
            return receipt;
        }

        private long Supply()
        {
            long total = 0;
            foreach (var product in _productsAvailable)
            {
                total += (int) product.TotalSupply;
            }

            return total;
        }



        public ReceiptModel BuyMaxProductsForMaxPricePerPiece(ProductRequestModel buyRequest)
        {
            decimal maxPrice = buyRequest.MaxPrice;
            int amount = buyRequest.MaxAmount;
            //int amountToRequestDemandDueToMissingSupply = 0;
            int demandAlreadyRequestedDueTooHighPrice = 0;
            List<ProductController> productsToRequestMissingSupply = new();
            var matchingProducts = _productsAvailable.ToList();
            matchingProducts = matchingProducts.OrderBy(x => x.Price).ToList();

            ReceiptModel receipt = new ReceiptModel();
            foreach (var p in matchingProducts)
            {
                var tempReciept = p.BuyMaxAmount(amount, maxPrice);
                int amountBought = tempReciept.AmountBought;
                receipt.AmountBought += amountBought;
                receipt.TotalPricePaid += tempReciept.TotalPricePaid;
                if (amountBought < amount)
                {
                    amount -= amountBought;
                    /*if (tempReciept.BuyRejectType == BuyRejectType.MissingSupply)
                    {
                        productsToRequestMissingSupply.Add(p);
                    }
                    else if(tempReciept.BuyRejectType == BuyRejectType.TooExpensive)
                    {
                        p.RequestDemandMissing(amount, buyRequest.DayOfMonthDivisor, BuyRejectType.TooExpensive);
                        demandAlreadyRequestedDueTooHighPrice += amount;
                    }*/
                }
                else
                {
                    amount = 0;
                    break;
                }
            }

            /*
    
            int unrequestedDemandLeft = amount - demandAlreadyRequestedDueTooHighPrice;
    
            if (unrequestedDemandLeft > 0)
            {
                int productCount = productsToRequestMissingSupply.Count;
    
                foreach (var pr in productsToRequestMissingSupply)
                {
                    pr.RequestDemandMissing(unrequestedDemandLeft / productCount, buyRequest.DayOfMonthDivisor, BuyRejectType.MissingSupply);
                }
            }
            */
            _currentSales += receipt.AmountBought;
            return receipt;
        }

        public ReceiptModel BuyMaxProducts(ProductRequestModel buyRequest)
        {
            int amount = buyRequest.MaxAmount;
            var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();

            ReceiptModel receipt = new ReceiptModel();
            foreach (var p in matchingProducts.Where(p => p.TotalSupply != 0))
            {
                if (amount < 0)
                    throw new Exception();
                var tempReciept = p.BuyMaxAmount(amount);
                int amountBought = tempReciept.AmountBought;
                receipt.AmountBought += amountBought;
                receipt.TotalPricePaid += tempReciept.TotalPricePaid;
                if (amountBought < amount)
                {
                    amount -= amountBought;
                }
                else
                {
                    amount = 0;
                    break;
                }
            }

            /*if (amount > 0)
            {
                int productCount = _productsAvailable.Count;
    
                foreach (var pr in _productsAvailable)
                {
                    pr.RequestDemandMissing(amount / productCount, BuyRejectType.MissingSupply);
                }
            }*/
            _currentSales += receipt.AmountBought;
            return receipt;
        }

        /*private void RequestDemandMissing(int amount, int dayOfMonthDivisor)
        {
            CurrentUnfulfilledDemand += amount;
            CurrentUnfulfilledDemand /= dayOfMonthDivisor;
        }*/

        public void ReportProduction(long amount)
        {
            _currentProduction += amount;
        }

        public void ReportDemand(long amount)
        {
            _currentUnfullfilledDemand += amount;
        }

        public void Reset()
        {
            /*foreach (var p in _productsAvailable)
            {
                p.Reset();
            }*/

            _dataRepository.Production.Add(_currentProduction);
            _dataRepository.Sales.Add(_currentSales);
            _dataRepository.Demand.Add(_currentUnfullfilledDemand);

            _currentSales = 0;
            _currentProduction = 0;
            _currentUnfullfilledDemand = 0;
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            var aggregates = new List<decimal>();
            decimal count = 0;
            foreach (var product in _productsAvailable)
            {
                aggregates.Add(product.TotalSupply * product.Price);
                //TotalSupply += count;
                count += product.TotalSupply;
            }

            AveragePrice = count == 0 ? _defaultPrice : aggregates.Sum() / count;
        }
    }
}