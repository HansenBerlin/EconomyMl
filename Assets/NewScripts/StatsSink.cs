using System.Linq;
using TMPro;
using UnityEngine;

namespace NewScripts
{
    public class StatsSink : MonoBehaviour
    {
        public TextMeshProUGUI workerMoneyText;
        public TextMeshProUGUI workerDemandFullfilledText;
        public TextMeshProUGUI workerEmploymentQuoteText;
        public TextMeshProUGUI companyReservesText;
        public TextMeshProUGUI companyLifetimeText;
        public TextMeshProUGUI companyWagesText;
        public TextMeshProUGUI companyPricesText;
        public TextMeshProUGUI companyStockText;
        public TextMeshProUGUI circulatingMoneyText;

        public void UpdateStats()
        {
            SetCirculatingMoneyText();
            SetCompanyLifetimeText();
            SetCompanyPricesText();
            SetCompanyReservesText();
            SetCompanyStockText();
            SetCompanyWagesText();
            SetWorkerTexts();
        }
        
        private void SetCirculatingMoneyText()
        {
            var workers = ServiceLocator.Instance.LaborMarketService.Workers;
            var companys = ServiceLocator.Instance.Companys;
            decimal workersTotal = workers.Select(x => x.Money).Sum();
            decimal companiesTotalProfit = companys.Select(x => x.ProfitInMonth).Sum();
            decimal companiesTotalLiquidity = companys.Select(x => x.Liquidity).Sum();
            decimal companiesTotal = companiesTotalLiquidity + companiesTotalProfit;
            string text = $"{workersTotal:0} | {companiesTotal:0} | {workersTotal + companiesTotal:0}";
            circulatingMoneyText.GetComponent<TextMeshProUGUI>().text = text;
        }

        private void SetWorkerTexts()
        {
            var workers = ServiceLocator.Instance.LaborMarketService.Workers;
            decimal avg = workers.Select(x => x.Money).Average();
            decimal min = workers.Select(x => x.Money).Min();
            decimal max = workers.Select(x => x.Money).Max();
            BuildText(workerMoneyText, min, max, avg, true);

            decimal employed = workers.Count(x => x.HasJob);
            decimal quote = employed / workers.Count;
            string quoteText = $"{quote * 100:0.##} %";
            workerEmploymentQuoteText.GetComponent<TextMeshProUGUI>().text = quoteText;
            
            decimal avgB = workers.Select(x => (decimal)x.DemandFulfilled).Average();
            decimal minB = workers.Select(x => (decimal)x.DemandFulfilled).Min();
            decimal maxB = workers.Select(x => (decimal)x.DemandFulfilled).Max();
            BuildText(workerDemandFullfilledText, minB, maxB, avgB);
        }
        
        private void SetCompanyWagesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            decimal avg = companys.Select(x => x.WageRate).Average();
            decimal min = companys.Select(x => x.WageRate).Min();
            decimal max = companys.Select(x => x.WageRate).Max();
            BuildText(companyWagesText, min, max, avg, true);
        }
        
        private void SetCompanyStockText()
        {
            var companys = ServiceLocator.Instance.Companys;
            decimal avg = companys.Select(x => (decimal)x.ProductStock).Average();
            decimal min = companys.Select(x => x.ProductStock).Min();
            decimal max = companys.Select(x => x.ProductStock).Max();
            BuildText(companyStockText, min, max, avg);
        }
        
        private void SetCompanyPricesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            decimal avg = companys.Select(x => x.ProductPrice).Average();
            decimal min = companys.Select(x => x.ProductPrice).Min();
            decimal max = companys.Select(x => x.ProductPrice).Max();
            BuildText(companyPricesText, min, max, avg, true);
        }
        
        private void SetCompanyReservesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            decimal avgR = companys.Select(x => x.ProfitInMonth).Average();
            decimal minR = companys.Select(x => x.ProfitInMonth).Min();
            decimal maxR = companys.Select(x => x.ProfitInMonth).Max();
            decimal avgL = companys.Select(x => x.Liquidity).Average();
            decimal minL = companys.Select(x => x.Liquidity).Min();
            decimal maxL = companys.Select(x => x.Liquidity).Max();
            BuildText(companyReservesText, minR + minL, maxR + maxL, avgR + avgL);
        }

        private void SetCompanyLifetimeText()
        {
            var companys = ServiceLocator.Instance.Companys;
            decimal avg = companys.Select(x => (decimal)x.LifetimeMonths).Average();
            decimal min = companys.Select(x => x.LifetimeMonths).Min();
            decimal max = companys.Select(x => x.LifetimeMonths).Max();
            BuildText(companyLifetimeText, min, max, avg, true);
        }

        private void BuildText(TextMeshProUGUI textField, decimal min, decimal max, decimal avg, bool roundTwoDecimals = false)
        {
            string text;
            if (roundTwoDecimals)
            {
                text = $"{min:0.##} | {avg:0.##} | {max:0.##}";
            }
            else
            {
                text = $"{min:0} | {avg:0} | {max:0}";
            }

            textField.GetComponent<TextMeshProUGUI>().text = text;
        }
    }
}