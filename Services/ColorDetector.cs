using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using WeBook.Models;

namespace WeBook.Services
{
    public class ColorDetector
    {
        private readonly IWebDriver _driver;

        public ColorDetector(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        /// <summary>
        /// Finds a pixel of the specified RGB color within the canvas,
        /// and returns both internal canvas coordinates and viewport coordinates (scaled).
        /// </summary>
        public SeatSelectionResult FindColorCoordinate(int r, int g, int b, bool isSeatMode)
        {
            string script = @"
                var canvas = document.querySelector('canvas');
                if (!canvas) return { success: false, error: 'No canvas found' };

                try {
                    var ctx = canvas.getContext('2d', { willReadFrequently: true });
                    var width = canvas.width;
                    var height = canvas.height;
                    var data = ctx.getImageData(0, 0, width, height).data;

                    var hits = [];
                    var tolerance = " + Config.AppConfig.ColorTolerance + @";
                    var isSeatMode = " + isSeatMode.ToString().ToLower() + @";
                    var seatStartY = height * " + Config.AppConfig.SeatAreaStart + @";
                    var sectionLimitY = height * " + Config.AppConfig.SectionAreaTopLimit + @";

                    for (var i = 0; i < data.length; i += 4) {
                        var rr = data[i];
                        var gg = data[i+1];
                        var bb = data[i+2];

                        if (Math.abs(rr - " + r + @") < tolerance &&
                            Math.abs(gg - " + g + @") < tolerance &&
                            Math.abs(bb - " + b + @") < tolerance) {

                            var x = (i/4) % width;
                            var y = Math.floor((i/4) / width);

                            // Filter based on mode
                            if (isSeatMode && y < seatStartY) continue;
                            if (!isSeatMode && y > sectionLimitY) continue;

                            hits.push({ x: x, y: y, r: rr, g: gg, b: bb });

                            if (hits.length >= 100) break;
                        }
                    }

                    if (hits.length === 0) {
                        return { success: false, error: 'No matching colors found in target area' };
                    }

                    var pick = hits[Math.floor(Math.random() * hits.length)];
                    
                    // Compute viewport coordinates (scaled to actual displayed size)
                    var rect = canvas.getBoundingClientRect();
                    var scaleX = rect.width / canvas.width;
                    var scaleY = rect.height / canvas.height;
                    var vx = pick.x * scaleX;
                    var vy = pick.y * scaleY;
                    
                    return { 
                        success: true, 
                        x: pick.x, 
                        y: pick.y,
                        vx: vx,
                        vy: vy,
                        r: pick.r, 
                        g: pick.g, 
                        b: pick.b,
                        totalHits: hits.length 
                    };
                    
                } catch (error) {
                    return { success: false, error: error.toString() };
                }
            ";

            try
            {
                var executor = (IJavaScriptExecutor)_driver;
                var result = executor.ExecuteScript(script) as Dictionary<string, object>;

                if (result != null && result.ContainsKey("success") && (bool)result["success"])
                {
                    return SeatSelectionResult.SuccessResult(
                        Convert.ToInt32(result["x"]),
                        Convert.ToInt32(result["y"]),
                        Convert.ToDouble(result["vx"]),
                        Convert.ToDouble(result["vy"]),
                        Convert.ToInt32(result["r"]),
                        Convert.ToInt32(result["g"]),
                        Convert.ToInt32(result["b"]),
                        Convert.ToInt32(result["totalHits"])
                    );
                }

                string error = result?.ContainsKey("error") == true ? result["error"].ToString() : "Unknown error";
                return SeatSelectionResult.FailureResult(error);
            }
            catch (Exception ex)
            {
                return SeatSelectionResult.FailureResult(ex.Message);
            }
        }
    }
}