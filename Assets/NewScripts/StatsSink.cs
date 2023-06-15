using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
            double workersTotal = workers.Select(x => x.Money).Sum();
            double companiesTotal = companys.Select(x => x.ReserveAmount).Sum();
            string text = $"{workersTotal:0} | {companiesTotal:0} | {workersTotal + companiesTotal:0}";
            circulatingMoneyText.GetComponent<TextMeshProUGUI>().text = text;
        }

        private void SetWorkerMoneyText()
        {
            var workers = ServiceLocator.Instance.LaborMarketService.Workers;
            double avg = workers.Select(x => x.Money).Average();
            double min = workers.Select(x => x.Money).Min();
            double max = workers.Select(x => x.Money).Max();
            BuildText(workerMoneyText, min, max, avg, true);
        }
        
        private void SetCompanyWagesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = companys.Select(x => x.WageRate).Average();
            double min = companys.Select(x => x.WageRate).Min();
            double max = companys.Select(x => x.WageRate).Max();
            BuildText(companyWagesText, min, max, avg, true);
        }
        
        private void SetCompanyStockText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = companys.Select(x => x.ProductStock).Average();
            double min = companys.Select(x => x.ProductStock).Min();
            double max = companys.Select(x => x.ProductStock).Max();
            BuildText(companyStockText, min, max, avg);
        }
        
        private void SetCompanyPricesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = companys.Select(x => x.ProductPrice).Average();
            double min = companys.Select(x => x.ProductPrice).Min();
            double max = companys.Select(x => x.ProductPrice).Max();
            BuildText(companyPricesText, min, max, avg, true);
        }
        
        private void SetCompanyReservesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = companys.Select(x => x.ReserveAmount).Average();
            double min = companys.Select(x => x.ReserveAmount).Min();
            double max = companys.Select(x => x.ReserveAmount).Max();
            BuildText(companyReservesText, min, max, avg);
        }

        private void SetCompanyLifetimeText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double totalLifetime = 0;
            foreach (var company in companys)
            {
                totalLifetime += company.LifetimeMonths;
            }

            double avg = totalLifetime / companys.Count;
            double min = companys.Select(x => x.LifetimeMonths).Min();
            double max = companys.Select(x => x.LifetimeMonths).Max();
            BuildText(companyLifetimeText, min, max, avg);
        }

        private void BuildText(TextMeshProUGUI textField, double min, double max, double avg, bool roundTwoDecimals = false)
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