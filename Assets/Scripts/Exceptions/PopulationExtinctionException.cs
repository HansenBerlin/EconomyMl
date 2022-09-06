using System;
using Assets.Scripts.Controller;

namespace Assets.Scripts.Exceptions
{



    public class PopulationExtinctionException : Exception
    {
        public PopulationExtinctionException()
        {
        }

        public PopulationExtinctionException(string message) : base(message)
        {
            ConsoleColorPrinter.WriteException(message);
        }

        public PopulationExtinctionException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}