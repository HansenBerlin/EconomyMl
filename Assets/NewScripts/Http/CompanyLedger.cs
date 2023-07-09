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
        public double price;
        public int workers;
        public double wage;
        public int sales;
        public int production;
        public int lifetime;
        public int openPositions;
        public bool isTraining;
        public int marketDemand;
        public double marketBidPrice;
    }
}

