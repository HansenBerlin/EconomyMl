using System;
using Assets.Scripts.Controller;

namespace Assets.Scripts.Exceptions
{



    public class PriceCalculationException : Exception
    {
        public PriceCalculationException()
        {
        }

        public PriceCalculationException(string message) : base(message)
        {
            ConsoleColorPrinter.WriteException(message);
        }

        public PriceCalculationException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}