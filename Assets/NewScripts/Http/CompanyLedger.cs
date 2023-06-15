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
        public decimal liquidity;
        public decimal profit;
        public int workers;
        public decimal wage;
        public int sales;
        public int stock;
        public int lifetime;
        public bool extinct;
    }
}

