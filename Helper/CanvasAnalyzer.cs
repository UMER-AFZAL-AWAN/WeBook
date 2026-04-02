using OpenQA.Selenium;

namespace WeBook.Helper
{

    public static class CanvasAnalyzer
    {
        public static SeatLayout ExtractSeats(IWebDriver driver, int targetR, int targetG, int targetB, int tolerance = 15, int clusterDistance = 12)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
                var targetR = arguments[0], targetG = arguments[1], targetB = arguments[2], tolerance = arguments[3], clusterDist = arguments[4];
                var canvas = document.querySelector('canvas');
                if (!canvas) return null;
                var ctx = canvas.getContext('2d', {willReadFrequently: true});
                var width = canvas.width;
                var height = canvas.height;
                var imgData = ctx.getImageData(0, 0, width, height);
                var data = imgData.data;

                // 1. Collect all matching pixels
                var points = [];
                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        var idx = (y * width + x) * 4;
                        if (Math.abs(data[idx] - targetR) <= tolerance &&
                            Math.abs(data[idx+1] - targetG) <= tolerance &&
                            Math.abs(data[idx+2] - targetB) <= tolerance) {
                            points.push({x: x, y: y});
                        }
                    }
                }

                // 2. Cluster points into seats
                var seats = [];
                var used = new Array(points.length).fill(false);
                for (var i = 0; i < points.length; i++) {
                    if (used[i]) continue;
                    var cluster = [points[i]];
                    used[i] = true;
                    for (var j = i+1; j < points.length; j++) {
                        if (used[j]) continue;
                        // check if close to any point in cluster
                        var close = false;
                        for (var k = 0; k < cluster.length; k++) {
                            var dx = cluster[k].x - points[j].x;
                            var dy = cluster[k].y - points[j].y;
                            if (Math.hypot(dx, dy) <= clusterDist) {
                                close = true;
                                break;
                            }
                        }
                        if (close) {
                            cluster.push(points[j]);
                            used[j] = true;
                        }
                    }
                    // compute bounding box and centre
                    var minX = Math.min(...cluster.map(p => p.x));
                    var maxX = Math.max(...cluster.map(p => p.x));
                    var minY = Math.min(...cluster.map(p => p.y));
                    var maxY = Math.max(...cluster.map(p => p.y));
                    var centreX = (minX + maxX) / 2;
                    var centreY = (minY + maxY) / 2;
                    seats.push({
                        centreX: centreX,
                        centreY: centreY,
                        minX: minX,
                        maxX: maxX,
                        minY: minY,
                        maxY: maxY,
                        width: maxX - minX,
                        height: maxY - minY
                    });
                }

                // 3. Group into rows (by Y)
                seats.sort((a,b) => a.centreY - b.centreY);
                var rows = [];
                var currentRow = [];
                var rowThreshold = 15; // max Y diff to be same row
                var lastY = null;
                for (var i = 0; i < seats.length; i++) {
                    var seat = seats[i];
                    if (lastY === null || Math.abs(seat.centreY - lastY) > rowThreshold) {
                        if (currentRow.length > 0) rows.push(currentRow);
                        currentRow = [seat];
                        lastY = seat.centreY;
                    } else {
                        currentRow.push(seat);
                    }
                }
                if (currentRow.length > 0) rows.push(currentRow);

                // sort seats within each row by X
                rows.forEach(row => row.sort((a,b) => a.centreX - b.centreX));

                // assign row/col indices
                for (var r = 0; r < rows.length; r++) {
                    for (var c = 0; c < rows[r].length; c++) {
                        rows[r][c].rowIndex = r;
                        rows[r][c].colIndex = c;
                    }
                }

                return {
                    rows: rows,
                    allSeats: seats
                };
            ";

            var result = js.ExecuteScript(script, targetR, targetG, targetB, tolerance, clusterDistance) as Dictionary<string, object>;
            if (result == null) return null;

            var layout = new SeatLayout();
            // Deserialize rows
            var rowsList = result["rows"] as List<object>;
            if (rowsList != null)
            {
                foreach (var rowObj in rowsList)
                {
                    var rowSeats = new List<Seat>();
                    var seatsList = rowObj as List<object>;
                    if (seatsList != null)
                    {
                        foreach (var seatObj in seatsList)
                        {
                            var dict = seatObj as Dictionary<string, object>;
                            if (dict != null)
                            {
                                rowSeats.Add(new Seat
                                {
                                    CentreX = Convert.ToDouble(dict["centreX"]),
                                    CentreY = Convert.ToDouble(dict["centreY"]),
                                    MinX = Convert.ToDouble(dict["minX"]),
                                    MaxX = Convert.ToDouble(dict["maxX"]),
                                    MinY = Convert.ToDouble(dict["minY"]),
                                    MaxY = Convert.ToDouble(dict["maxY"]),
                                    RowIndex = dict.ContainsKey("rowIndex") ? Convert.ToInt32(dict["rowIndex"]) : -1,
                                    ColIndex = dict.ContainsKey("colIndex") ? Convert.ToInt32(dict["colIndex"]) : -1
                                });
                            }
                        }
                    }
                    layout.Rows.Add(rowSeats);
                }
            }

            // Deserialize allSeats
            var allSeatsList = result["allSeats"] as List<object>;
            if (allSeatsList != null)
            {
                foreach (var seatObj in allSeatsList)
                {
                    var dict = seatObj as Dictionary<string, object>;
                    if (dict != null)
                    {
                        layout.AllSeats.Add(new Seat
                        {
                            CentreX = Convert.ToDouble(dict["centreX"]),
                            CentreY = Convert.ToDouble(dict["centreY"]),
                            MinX = Convert.ToDouble(dict["minX"]),
                            MaxX = Convert.ToDouble(dict["maxX"]),
                            MinY = Convert.ToDouble(dict["minY"]),
                            MaxY = Convert.ToDouble(dict["maxY"])
                        });
                    }
                }
            }

            return layout;
        }
    }
}