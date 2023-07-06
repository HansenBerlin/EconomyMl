using System.Collections.Generic;
using System.Linq;
using NewScripts.Game.Services;
using NewScripts.Interfaces;

namespace NewScripts.Game.Entities
{
    public class Government
    {
        public decimal Liquidity { get; private set; }
        public decimal TaxRate { get; private set; }
        public decimal SubsidyRate { get; private set; }
        public int FoodSupply { get; private set; }
        private List<ICompany> _companies => ServiceLocator.Instance.Companys;
        
        public decimal PayTaxes(decimal amount)
        {
            var tax = amount * TaxRate;
            Liquidity += tax;
            return tax;
        }
        
        public void PaySubsidy()
        {
            var startups = _companies.Where(x => x.LifetimeMonths < 12).ToList();
            if(startups.Count == 0)
            {
                return;
            }
            
            decimal subsidityPerCompany = Liquidity * SubsidyRate / startups.Count;
            foreach (var company in startups)
            {
                company.Liquidity += subsidityPerCompany;
                Liquidity -= subsidityPerCompany;
            }
        }
        
        
    }
}