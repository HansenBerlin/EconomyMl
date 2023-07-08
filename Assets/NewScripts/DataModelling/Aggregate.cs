namespace NewScripts.DataModelling
{
    public class Aggregate
    {
        protected Aggregate(int month, int year)
        {
            Month = month;
            Year = year;
        }

        public int Month { get; }
        public int Year { get; }
    }
}