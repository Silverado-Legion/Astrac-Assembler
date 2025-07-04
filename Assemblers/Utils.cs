namespace Astrac.Assemblers
{
    internal static class Utils
    {
        public static void ResetConColors()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void SetConColors(ConsoleColor bg, ConsoleColor fg)
        {
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        public static void WriteError(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void EndProgram()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Press any key to exit...");
            Console.CursorVisible = false;
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void FatalEndProgram(string message)
        {
            Console.Beep();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL: {message}");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Press any key to exit...");
            Console.CursorVisible = false;
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
