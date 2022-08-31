namespace EconomyBase.Settings
{



    public record PoliciesWrapper(AgeBoundaryPolicy AgeBoundaries, EducationBoundaryPolicy EducationBoundaries,
        WorkerPolicy WorkerPolicies, FederalServicesPolicy FederalPolicies)
    {
        public AgeBoundaryPolicy AgeBoundaries { get; } = AgeBoundaries;
        public EducationBoundaryPolicy EducationBoundaries { get; } = EducationBoundaries;
        public WorkerPolicy WorkerPolicies { get; } = WorkerPolicies;
        public FederalServicesPolicy FederalPolicies { get; } = FederalPolicies;
    }
}