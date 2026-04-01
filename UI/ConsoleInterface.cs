using System;

namespace WeBook.UI
{
    public static class ConsoleInterface
    {
        public static string GetUrl()
        {
            Console.Write("🔗 Enter Event URL: ");
            return Console.ReadLine() ?? "";
        }

        public static int GetSeatCount()
        {
            Console.Write("🪑 How many seats? ");
            if (int.TryParse(Console.ReadLine(), out int count)) return count;
            return 1; // Default to 1 if input is invalid
        }
    }
}