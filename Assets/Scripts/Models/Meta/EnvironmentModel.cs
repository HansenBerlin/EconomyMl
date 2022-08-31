namespace EconomyBase.Models.Meta
{



    public class EnvironmentModel
    {
        public int Month { get; set; }
        public int Day { get; set; }
        public int Year => (int) Math.Floor((double) Month / 12);
        public string CountryName { get; }

        public EnvironmentModel(string countryName, int month)
        {
            CountryName = countryName;
            Month = month;
        }

    }
}