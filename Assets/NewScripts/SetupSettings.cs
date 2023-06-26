namespace NewScripts
{
    public class SetupSettings
    {
        public bool IsThrottled { get; set; }
        public bool IsTraining { get; set; }
        public bool WriteToDatabase { get; set; }
        public const int DaysPerMonth = 20;
        public const int Workers = 1000;
        public const int Companys = 25;
        public int OutputMultiplier { get; } = 100;

    }
}