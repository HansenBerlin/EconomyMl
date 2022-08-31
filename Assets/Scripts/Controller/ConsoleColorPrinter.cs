namespace EconomyBase.Controller
{



    public static class ConsoleColorPrinter
    {
        public static bool Print = false;

        public static void Write(string content, ConsoleColor color)
        {
            if (!Print) return;
            var col = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = col;
        }

        public static void WriteException(string content)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ EXCEPTION ] {content}");
            Console.ForegroundColor = col;
        }
    }
}