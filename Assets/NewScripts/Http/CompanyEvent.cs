using System;

namespace NewScripts.Http
{
    [Serializable] 
    public class CompanyEvent
    {
        public string sessionId;
        public int id;
        public int companyId;
        public int month;
        public int year;
        public decimal actPrice;
        public decimal actWage;
        public int actWorker;
    }
}

