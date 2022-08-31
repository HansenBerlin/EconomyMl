namespace EconomyBase.Settings
{



    public record PersonResourceDemandSettings(float DemandWorkerAge, float DemandChild, float DemandRetired)
    {
        public float DemandWorkerAge { get; } = DemandWorkerAge;
        public float DemandChild { get; } = DemandChild;
        public float DemandRetired { get; } = DemandRetired;
    }
}