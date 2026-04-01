namespace WeBook.Utilities
{
    public static class Logger
    {
        public static void Log(string message)
        {
            System.Console.WriteLine($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}