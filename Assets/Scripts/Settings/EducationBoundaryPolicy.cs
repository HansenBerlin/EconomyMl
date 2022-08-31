namespace EconomyBase.Settings
{



    public record EducationBoundaryPolicy(int AgeToStartSchool, int MinYearsInSchool, int MaxYearsInSchool)
    {
        public int AgeToStartSchool { get; } = AgeToStartSchool;
        public int MinYearsInSchool { get; } = MinYearsInSchool;
        public int MaxYearsInSchool { get; } = MaxYearsInSchool;
    }
}