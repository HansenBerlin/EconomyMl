using Assets.Scripts.Enums;

namespace Assets.Scripts.Settings
{



    public static class IdGenerator
    {
        private static int IdRunner = 1;

        public static string Create(int monthCreated, string country, ProductType product)
        {
            string id = $"{country}-{IdRunner++}-{product}-{monthCreated}";
            return id;
        }
    }
}