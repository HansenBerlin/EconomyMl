using System;

namespace NewScripts.Http
{
    [Serializable] 
    public class CompanyLedger
    {
        public string sessionId;
        public int id;
        public int companyId;
        public int month;
        public int year;
        public double liquidity;
        public double profit;
        public double priceFood;
        public double priceLux;
        public int workers;
        public double wage;
        public int salesFood;
        public int salesLux;
        public int productionFood;
        public int productionLux;
        public int stockFood;
        public int stockLux;
        public int lifetime;
        public int openPositions;
        public bool isTraining;
        public int marketDemandFood;
        public double marketBidPriceFood;
        public int marketDemandLux;
        public double marketBidPriceLux;
        public double ressourceAllocation;
    }
}

