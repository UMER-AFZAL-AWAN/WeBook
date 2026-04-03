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
            int count = 0;
            bool isValid = false;

            // This loop will run until the user enters exactly 1, 2, 3, 4, or 5
            while (!isValid)
            {
                Console.Write("?? How many seats? (Select 1 to 5): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out count))
                {
                    if (count >= 1 && count <= 5)
                    {
                        isValid = true; // Input is perfect, exit the loop
                    }
                    else if (count <= 0)
                    {
                        Console.WriteLine("❌ Error: Quantity cannot be zero or negative. Please try again.");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Restricted: Website limits booking to a maximum of 5 seats. Please choose 1-5.");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Invalid input: Please enter a number between 1 and 5.");
                }
            }

            return count;
        }
    }
}