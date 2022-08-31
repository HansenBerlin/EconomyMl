namespace Settings
{



    public record AgeBoundaryPolicy(int AdultMinAge, int WorkerMaxAge)
    {
        public int AdultMinAge { get; } = AdultMinAge;
        public int WorkerMaxAge { get; } = WorkerMaxAge;
    }
}