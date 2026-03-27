using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook
{
    public static class Logger
    {
        private static readonly string LogFile = "webook_log.txt";

        public static void Log(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Console.WriteLine(line);
            File.AppendAllText(LogFile, line + Environment.NewLine);
        }

        public static void LogError(Exception ex)
        {
            Log("ERROR: " + ex.Message);
            Log("STACK: " + ex.StackTrace);
        }
    }

}
