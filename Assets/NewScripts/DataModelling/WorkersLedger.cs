namespace NewScripts.DataModelling
{
    public class WorkersLedger
    {
        public WorkersLedger(int startCount, decimal offeredWage, decimal averageWage)
        {
            StartCount = startCount;
            OfferedWage = offeredWage;
            AverageWage = averageWage;
        }

        public int StartCount { get; }
        public int EndCount { get; set; }
        public decimal OfferedWage { get; }
        public int Hired { get; set; }
        public int FiredByDecision { get; set; }
        public int FiredByLackOfFunds { get; set; }
        public int Quit { get; set; }
        public int OpenPositions { get; set; }
        public int ReducedPaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal AverageWage { get; }
    }
}