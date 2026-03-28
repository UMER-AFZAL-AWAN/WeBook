using System;

namespace WeBook.Helpers
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public static void Success(string message)
        {
            Console.WriteLine($"✅ {message}");
        }

        public static void Warning(string message)
        {
            Console.WriteLine($"⚠️ {message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"❌ {message}");
        }

        public static void Debug(string message)
        {
            Console.WriteLine($"🔍 {message}");
        }

        public static void Step(string message)
        {
            Console.WriteLine($"\n📌 {message}");
        }

        public static void Click(string message)
        {
            Console.WriteLine($"  🎯 {message}");
        }

        public static void Separator()
        {
            Console.WriteLine(new string('-', 60));
        }
    }
}