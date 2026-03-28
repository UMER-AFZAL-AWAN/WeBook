namespace WeBook.Config
{
    public static class AppConfig
    {
        // Login Credentials
        public static string Email = "cabnipcar@bangban.uk";
        public static string Password = "Aa@123456789";
        public static string Url = "https://webook.com/en/events/rsl-al-khaleej-vs-al-hilal-387468/book";

        // Colors
        public static int[] AvailableColor = { 151, 211, 94 }; // RGB for available seats/sections

        // Selection Settings
        public static int MaxSeats = 5;
        public static int ColorTolerance = 25;

        // Timeouts (in milliseconds)
        public static int ShortDelay = 500;
        public static int MediumDelay = 1000;
        public static int LongDelay = 2000;
        public static int ExtraLongDelay = 5000;

        // Canvas Areas (percentage)
        public static double SectionAreaTopLimit = 0.3; // Top 30% for sections
        public static double SeatAreaStart = 0.3; // Start at 30% for seats
    }
}