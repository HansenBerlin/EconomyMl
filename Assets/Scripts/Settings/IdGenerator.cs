using Enums;

namespace Settings
{
    public static class IdGenerator
    {
        private static int _idRunner = 1;

        public static string Create(int monthCreated, string country, ProductType product)
        {
            var id = $"{country}-{_idRunner++}-{product}-{monthCreated}";
            return id;
        }
    }
}