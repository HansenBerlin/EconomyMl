using System.Linq;
using TMPro;
using UnityEngine;

namespace NewScripts
{
    public class StatsSink : MonoBehaviour
    {
        public TextMeshProUGUI workerMoneyText;
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
            SetWorkerMoneyText();
        }
        
        private void SetCirculatingMoneyText()
        {
            var workers = ServiceLocator.Instance.LaborMarketService.Workers;
            var companys = ServiceLocator.Instance.Companys;
            decimal workersTotal = workers.Select(x => x.Money).Sum();
            decimal companiesTotal = companys.Select(x => x.ProfitInMonth).Sum();
            string text = $"{workersTotal:0} | {companiesTotal:0} | {workersTotal + companiesTotal:0}";
            circulatingMoneyText.GetComponent<TextMeshProUGUI>().text = text;
        }

        private void SetWorkerMoneyText()
        {
            var workers = ServiceLocator.Instance.LaborMarketService.Workers;
            decimal avg = workers.Select(x => x.Money).Average();
            decimal min = workers.Select(x => x.Money).Min();
            decimal max = workers.Select(x => x.Money).Max();
            BuildText(workerMoneyText, min, max, avg, true);
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