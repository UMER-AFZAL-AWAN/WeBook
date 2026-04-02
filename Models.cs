namespace WeBook
{
    public class Category
    {
        public string Name { get; set; }
        public int[] Rgb { get; set; } // [R, G, B]
        public int AvailableSeats { get; set; }
    }

    public class Seat
    {
        public double CentreX { get; set; }
        public double CentreY { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public int RowIndex { get; set; }  // optional, computed during extraction
        public int ColIndex { get; set; }
    }

    public class SeatLayout
    {
        public List<List<Seat>> Rows { get; set; } = new List<List<Seat>>();
        public List<Seat> AllSeats { get; set; } = new List<Seat>();
    }

}
