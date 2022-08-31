namespace EconomyBase.Controller
{



    public class ConsoleSpinner
    {
        static string[,] _sequence;

        public int Delay { get; set; } = 1;

        int _totalSequences;
        int _counter;

        public ConsoleSpinner()
        {
            _counter = 0;
            _sequence = new string[,]
            {
                {"/", "-", "\\", "|"},
                {".", "o", "0", "o"},
                {"+", "x", "+", "x"},
                {"V", "<", "^", ">"},
                {".   ", "..  ", "... ", "...."},
                {"=>   ", "==>  ", "===> ", "====>"},
                // ADD YOUR OWN CREATIVE SEQUENCE HERE IF YOU LIKE
            };

            _totalSequences = _sequence.GetLength(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequenceCode"> 0 | 1 | 2 |3 | 4 | 5 </param>
        public void Turn(string displayMsg = "", int sequenceCode = 0)
        {
            _counter++;

            Thread.Sleep(Delay);

            sequenceCode = sequenceCode > _totalSequences - 1 ? 0 : sequenceCode;

            int counterValue = _counter % 4;

            string fullMessage = displayMsg + _sequence[sequenceCode, counterValue];
            int msglength = fullMessage.Length;

            Console.Write(fullMessage);

            Console.SetCursorPosition(Console.CursorLeft - msglength, Console.CursorTop);
        }
    }
}