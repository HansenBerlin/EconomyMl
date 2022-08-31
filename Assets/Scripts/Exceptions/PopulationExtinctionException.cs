using EconomyBase.Controller;

namespace EconomyBase.Exceptions
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