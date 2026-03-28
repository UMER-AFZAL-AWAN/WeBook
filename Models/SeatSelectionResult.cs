namespace WeBook.Models
{
    // In Models/SeatSelectionResult.cs
    public class SeatSelectionResult
    {
        public bool Success { get; set; }
        public int X { get; set; }          // internal canvas x
        public int Y { get; set; }          // internal canvas y
        public double ViewportX { get; set; } // scaled x for click
        public double ViewportY { get; set; } // scaled y for click
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int TotalHits { get; set; }
        public string Error { get; set; }

        public static SeatSelectionResult SuccessResult(int x, int y, double vx, double vy, int r, int g, int b, int totalHits)
        {
            return new SeatSelectionResult
            {
                Success = true,
                X = x,
                Y = y,
                ViewportX = vx,
                ViewportY = vy,
                R = r,
                G = g,
                B = b,
                TotalHits = totalHits
            };
        }

        public static SeatSelectionResult FailureResult(string error)
        {
            return new SeatSelectionResult
            {
                Success = false,
                Error = error
            };
        }
    }
}