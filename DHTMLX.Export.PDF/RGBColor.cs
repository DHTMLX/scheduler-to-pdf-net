using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
namespace DHTMLX.Export.PDF.Scheduler
{
    public class RGBColor
    {
        private static Dictionary<string, double[]> _parsedColors = new Dictionary<string, double[]>();

        public static double[] GetColor(string color)
        {
            if (_parsedColors.ContainsKey(color))
                return (double[])_parsedColors[color];
            var original = color;
            color = RGBColor.ProcessColorForm(color);
            var result = new double[3];
            var r = color.Substring(0, 2);
            var g = color.Substring(2, 2);
            var b = color.Substring(4, 2);

            result[0] = int.Parse(r, System.Globalization.NumberStyles.HexNumber) / 255.0;
            result[1] = int.Parse(g, System.Globalization.NumberStyles.HexNumber) / 255.0;
            result[2] = int.Parse(b, System.Globalization.NumberStyles.HexNumber) / 255.0;
            _parsedColors.Add(original, result);
            return result;
        }

        public static XColor GetXColor(string color)
        {
            var dblColor = GetColor(color);
            return XColor.FromArgb((int)Math.Floor(dblColor[0] * 255), (int)Math.Floor(dblColor[1] * 255), (int)Math.Floor(dblColor[2] * 255));
        }

        public static XColor GetXColor(double[] dblColor)
        {
            return XColor.FromArgb((int)Math.Floor(dblColor[0] * 255), (int)Math.Floor(dblColor[1] * 255), (int)Math.Floor(dblColor[2] * 255));
        }

        public static string ProcessColorForm(string color)
        {
            if (color.Equals("transparent"))
            {
                return "";
            }
            if (Regex.IsMatch(color, "#[0-9A-Fa-f]{6}"))
            {
                return color.Substring(1);
            }

            if (Regex.IsMatch(color, "[0-9A-Fa-f]{6}"))
            {
                return color;
            }

            var m3 = Regex.Match(color, "rgb\\s?\\(\\s?(\\d{1,3})\\s?,\\s?(\\d{1,3})\\s?,\\s?(\\d{1,3})\\s?\\)");

            if (m3.Length > 0)
            {
                var r = m3.Groups[1].Value;
                var g = m3.Groups[2].Value;
                var b = m3.Groups[3].Value;
                r = int.Parse(r).ToString("x");
                r = (r.Length == 1) ? "0" + r : r;
                g = int.Parse(g).ToString("x");
                g = (g.Length == 1) ? "0" + g : g;
                b = int.Parse(b).ToString("x");
                b = (b.Length == 1) ? "0" + b : b;
                color = r + g + b;
                return color;
            }
            return "";
        }
    }
}