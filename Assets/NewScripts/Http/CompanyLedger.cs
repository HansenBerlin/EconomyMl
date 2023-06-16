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
        public int workers;
        public double wage;
        public int sales;
        public int stock;
        public int lifetime;
        public bool extinct;
        public int emergencyRounds;
    }
}

