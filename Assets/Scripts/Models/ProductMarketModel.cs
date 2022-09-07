using System;
using System.Collections.Generic;
using System.Linq;
using Controller.RepositoryController;
using Enums;
using Unity.MLAgents;

namespace Models
{
    public class ProductMarketModel
    {
        private readonly List<ProductController> _productsAvailable = new();
        public readonly ProductType Type;
        private float _capital;
        private float _cpp;
        private long _currentUnfullfilledDemand;
        private decimal _defaultPrice;
        private long _lastUnfullfilledDemand;
        private float _moneyIn;
        private float _moneyOut;
        private float _price;
        private long _production;
        private long _sales;
        private int _workers;
        public string Id = Guid.NewGuid().ToString();

        public ProductMarketModel(ProductType type)
        {
            Type = type;
        }

        public decimal AveragePrice { get; private set; }

        public void AddProduct(ProductController product)
        {
            if (_defaultPrice == 0)
                _defaultPrice = product.Price;
            _productsAvailable.Add(product);
            UpdateProperties();
        }

        public void RemoveProduct(string id)
        {
            var product = _productsAvailable.First(x => x.Id == id);
            _productsAvailable.Remove(product);
            UpdateProperties();
        }

        public decimal GetMarketShare(string productId)
        {
            var product = _productsAvailable.FirstOrDefault(p => p.Id == productId);
            long salesThis = 0;
            if (_productsAvailable.Count == 1 && product != null) return 1;
            if (_productsAvailable.Count > 1 && product != null) salesThis = product.SalesLastMonth;

            long allSales = _productsAvailable.Sum(p => p.SalesLastMonth);

            decimal marketShare = allSales > 0 ? salesThis / allSales : 0;
            return marketShare;
        }

        public ReceiptModel BuyMaxProductsForMoney(ProductRequestModel buyRequest)
        {
            decimal moneyAmount = buyRequest.TotalSpendable;
            //var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();
            _productsAvailable.Sort((p1,p2)=>decimal.Compare(p1.Price,p2.Price));

            var receipt = new ReceiptModel();
            foreach (var p in _productsAvailable)
            {
                var tempReciept = p.BuyFor(moneyAmount);
                decimal moneyPaid = tempReciept.TotalPricePaid;
                receipt.AmountBought += tempReciept.AmountBought;
                receipt.TotalPricePaid += moneyPaid;
                moneyAmount -= moneyPaid;
                if (moneyAmount == 0) break;
            }

            return receipt;
        }

        public ReceiptModel BuyMaxProductsForMaxAmountSpended(ProductRequestModel buyRequest)
        {
            decimal moneyAmount = buyRequest.TotalSpendable;
            long maxAmount = buyRequest.MaxAmount;
            //var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();
            _productsAvailable.Sort((p1,p2)=>decimal.Compare(p1.Price,p2.Price));

            var receipt = new ReceiptModel();
            foreach (var p in _productsAvailable)
            {
                var tempReciept = p.BuyFor(moneyAmount, maxAmount);
                decimal moneyPaid = tempReciept.TotalPricePaid;
                long amountBought = tempReciept.AmountBought;
                receipt.AmountBought += tempReciept.AmountBought;
                receipt.TotalPricePaid += moneyPaid;
                maxAmount -= amountBought;
                moneyAmount -= moneyPaid;

                if (moneyAmount == 0 || maxAmount == 0) break;
            }

            return receipt;
        }

        private long Supply()
        {
            return _productsAvailable.Sum(product => product.TotalSupply);
        }


        public ReceiptModel BuyMaxProductsForMaxPricePerPiece(ProductRequestModel buyRequest)
        {
            decimal maxPrice = buyRequest.MaxPrice;
            long amount = buyRequest.MaxAmount;
            //var matchingProducts = _productsAvailable.ToList();
            //matchingProducts = matchingProducts.OrderBy(x => x.Price).ToList();
            _productsAvailable.Sort((p1,p2)=>decimal.Compare(p1.Price,p2.Price));


            var receipt = new ReceiptModel();
            foreach (var p in _productsAvailable)
            {
                if (maxPrice < p.Price)
                {
                    continue;
                }
                var tempReciept = p.BuyMaxAmount(amount, maxPrice);
                long amountBought = tempReciept.AmountBought;
                receipt.AmountBought += amountBought;
                receipt.TotalPricePaid += tempReciept.TotalPricePaid;
                if (amountBought < amount)
                    amount -= amountBought;
                else
                    break;
            }

            return receipt;
        }

        public ReceiptModel BuyMaxProducts(ProductRequestModel buyRequest)
        {
            long amount = buyRequest.MaxAmount;
            //var matchingProducts = _productsAvailable.OrderBy(x => x.Price).ToList();
            _productsAvailable.Sort((p1,p2)=>decimal.Compare(p1.Price,p2.Price));

            var receipt = new ReceiptModel();
            foreach (var p in _productsAvailable)
            {
                var tempReciept = p.BuyMaxAmount(amount);
                long amountBought = tempReciept.AmountBought;
                receipt.AmountBought += amountBought;
                receipt.TotalPricePaid += tempReciept.TotalPricePaid;
                if (amountBought < amount)
                    amount -= amountBought;
                else
                    break;
            }

            return receipt;
        }

        public void ReportStats(int workers, float capital, float moneyIn, float moneyOut,
            long production, long sales, float price, float cpp)
        {
            _workers += workers;
            _capital += capital;
            _moneyIn += moneyIn;
            _moneyOut += moneyOut;
            _production += production;
            _sales += sales;
            _price += price;
            _cpp += cpp;
        }

        private void WriteAndResetStats()
        {
            float cnt = _productsAvailable.Count;
            Academy.Instance.StatsRecorder.Add("workers/" + Type, _workers / cnt);
            Academy.Instance.StatsRecorder.Add("capital/" + Type, _capital / cnt);
            Academy.Instance.StatsRecorder.Add("moneyin/" + Type, _moneyIn / cnt);
            Academy.Instance.StatsRecorder.Add("moneyout/" + Type, _moneyOut / cnt);
            Academy.Instance.StatsRecorder.Add("production/" + Type, _production / cnt);
            Academy.Instance.StatsRecorder.Add("sales/" + Type, _sales / cnt);
            Academy.Instance.StatsRecorder.Add("price/" + Type, _price / cnt);
            Academy.Instance.StatsRecorder.Add("cpp/" + Type, _cpp / cnt);
            _workers = 0;
            _capital = 0;
            _moneyIn = 0;
            _moneyOut = 0;
            _production = 0;
            _sales = 0;
            _price = 0;
            _cpp = 0;
        }

        public void ReportDemand(long amount)
        {
            _currentUnfullfilledDemand += amount;
        }

        public void Reset()
        {
            WriteAndResetStats();
            _lastUnfullfilledDemand = _currentUnfullfilledDemand;
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
                count += product.TotalSupply;
            }

            AveragePrice = count == 0 ? _defaultPrice : aggregates.Sum() / count;
        }

        public long GetTotalUnfullfilledDemand()
        {
            return _lastUnfullfilledDemand;
        }
    }
}