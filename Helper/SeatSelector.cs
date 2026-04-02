using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Helper
{
    public static class SeatSelector
    {
        public static List<Seat> FindConsecutiveSeats(SeatLayout layout, int seatCount)
        {
            // Iterate through rows and find consecutive seats in each row
            foreach (var row in layout.Rows)
            {
                for (int i = 0; i <= row.Count - seatCount; i++)
                {
                    bool consecutive = true;
                    // Check that seats are adjacent (no large gaps)
                    for (int j = i; j < i + seatCount - 1; j++)
                    {
                        // If the gap between two seats is more than twice the average seat width, consider it a break
                        double avgWidth = (row[j].MaxX - row[j].MinX + row[j + 1].MaxX - row[j + 1].MinX) / 2;
                        if (row[j + 1].CentreX - row[j].CentreX > avgWidth * 2)
                        {
                            consecutive = false;
                            break;
                        }
                    }
                    if (consecutive)
                    {
                        return row.GetRange(i, seatCount);
                    }
                }
            }
            return null;
        }

        public static List<Seat> TakeFirstNSeats(List<Seat> seats, int count)
        {
            if (seats.Count >= count)
                return seats.Take(count).ToList();
            return null;
        }
    }

}
