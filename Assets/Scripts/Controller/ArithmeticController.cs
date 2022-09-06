using System;

namespace Assets.Scripts.Controller
{
    public static class ArithmeticController
    {
        public static int RoundToInt(double value)
        {
            return (int) Math.Round(value, MidpointRounding.AwayFromZero);

        }

        public static decimal RoundToDecimalWithTwoPlaces(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.ToEven);

        }

        public static decimal PercentChange(decimal newValue, decimal baseValue)
        {
            var change = baseValue == 0 ? newValue == 0 ? 0 : 1 : (newValue - baseValue) / baseValue;
            return change;
        }

        /*public static decimal PercentDiff(decimal compareValue, decimal againstValue)
        {
            var change = againstValue == 0 ? 0 : (newValue - baseValue) / baseValue;
            return change;
        }*/
    }
}