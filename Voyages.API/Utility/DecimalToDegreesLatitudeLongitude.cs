using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoyagesAPIService.Utility
{
    public class DecimalToDegreesLatitudeLongitude
    {

        public static string DDtoDMS(double coordinate, CoordinateType type)
        {
            // Set flag if number is negative
            bool neg = coordinate < 0d;

            // Work with a positive number
            coordinate = Math.Abs(coordinate);

            // Get d/m/s components
            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = Math.Round(coordinate);

            // Create padding character
            char pad;
            char.TryParse("0", out pad);

            // Create d/m/s strings
            string dd = d.ToString();
            string mm = m.ToString().PadLeft(2, pad);
            string ss = s.ToString().PadLeft(2, pad);

            // Append d/m/s
            string dms = string.Format("{0}° {1}' {2}\" ", dd, mm, ss);

            // Append compass heading
            switch (type)
            {
                case CoordinateType.longitude:
                    dms += neg ? "W" : "E";
                    break;
                case CoordinateType.latitude:
                    dms += neg ? "S" : "N";
                    break;
            }

            // Return formated string
            return dms;
        }

    }
    public enum CoordinateType { longitude, latitude };
}
