using System.Linq;
using TMPro;
using Unity.MLAgents;
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
            var workers = ServiceLocator.Instance.LaborMarket.Workers;
            var companys = ServiceLocator.Instance.Companys;
            double workersTotal = (double)workers.Select(x => x.Money).Sum();
            double companiesTotalLiquidity = (double)companys.Select(x => x.Liquidity).Sum();
            double companiesTotal = companiesTotalLiquidity;
            string text = $"{workersTotal:0} | {companiesTotal:0} | {workersTotal + companiesTotal:0}";
            circulatingMoneyText.GetComponent<TextMeshProUGUI>().text = text;
        }

        private void SetWorkerTexts()
        {
            var workers = ServiceLocator.Instance.LaborMarket.Workers;
            double avg = (double)workers.Select(x => x.Money).Average();
            double min = (double)workers.Select(x => x.Money).Min();
            double max = (double)workers.Select(x => x.Money).Max();
            BuildText(workerMoneyText, min, max, avg, true);

            double employed = workers.Count(x => x.HasJob);
            double quote = employed / workers.Count;
            string quoteText = $"{quote * 100:0.##} %";
            Academy.Instance.StatsRecorder.Add("Labor/WorkPercnt", (float)quote * 100);

            workerEmploymentQuoteText.GetComponent<TextMeshProUGUI>().text = quoteText;
            
            //double avgB = workers.Select(x => x.DemandFulfilled).Average();
            //double minB = workers.Select(x => x.DemandFulfilled).Min();
            //double maxB = workers.Select(x => x.DemandFulfilled).Max();
            //BuildText(workerDemandFullfilledText, minB, maxB, avgB);
        }
        
        private void SetCompanyWagesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = (double)companys.Select(x => x.OfferedWageRate).Average();
            double min = (double)companys.Select(x => x.OfferedWageRate).Min();
            double max = (double)companys.Select(x => x.OfferedWageRate).Max();
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
            double avg = (double)companys.Select(x => x.ProductPrice).Average();
            double min = (double)companys.Select(x => x.ProductPrice).Min();
            double max = (double)companys.Select(x => x.ProductPrice).Max();
            BuildText(companyPricesText, min, max, avg, true);
        }
        
        private void SetCompanyReservesText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avgL = (double)companys.Select(x => x.Liquidity).Average();
            double minL = (double)companys.Select(x => x.Liquidity).Min();
            double maxL = (double)companys.Select(x => x.Liquidity).Max();
            BuildText(companyReservesText,  minL, maxL,  avgL);
        }

        private void SetCompanyLifetimeText()
        {
            var companys = ServiceLocator.Instance.Companys;
            double avg = companys.Select(x => x.LifetimeMonths).Average();
            double min = companys.Select(x => x.LifetimeMonths).Min();
            double max = companys.Select(x => x.LifetimeMonths).Max();
            BuildText(companyLifetimeText, min, max, avg, true);
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