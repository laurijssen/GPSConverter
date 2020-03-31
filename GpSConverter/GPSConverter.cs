using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpSConverter
{
    class GPSConverter
    {
        double RDOriginX = 155E3;
        double RDOriginY = 463E3;
        double GpsOriginLat = 52.1551744;
        double GpsOriginLon = 5.38720621;

        List<List<double>> lat = new List<List<double>>(11) { new List<double>() { 0, 1, 3235.65389 },
                                                              new List<double>() { 2, 0, -32.58297 },
                                                              new List<double>() { 0, 2, -0.2475 },
                                                              new List<double>() { 2, 1, -0.84978 },
                                                              new List<double>() { 0, 3, -0.0665 },
                                                              new List<double>() { 2, 2, -0.01709 },
                                                              new List<double>() { 1, 0, -0.00738 },
                                                              new List<double>() { 4, 0, 0.0053 },
                                                              new List<double>() { 2, 3, -3.9E-4 },
                                                              new List<double>() { 4, 1, 3.3E-4 },
                                                              new List<double>() { 1, 1, -1.2E-4 } };

        List<List<double>> lon = new List<List<double>>(12) { new List<double>() { 1, 0, 5260.52916 },
                                                              new List<double>() { 1, 1, 105.94684 },
                                                              new List<double>() { 1, 2, 2.45656 },
                                                              new List<double>() { 3, 0, -0.81885 },
                                                              new List<double>() { 1, 3, 0.05594 },
                                                              new List<double>() { 3, 1, -0.05607 },
                                                              new List<double>() { 0, 1, 0.01199 },
                                                              new List<double>() { 3, 2, -0.00256 },
                                                              new List<double>() { 1, 4, 0.00128 },
                                                              new List<double>() { 0, 0, 2.2E-4 },
                                                              new List<double>() { 2, 0, -2.2E-4 },
                                                              new List<double>() { 5, 0, 2.6E-4 } };



        public GPSConverter()
        {
        }

        public double toLat(double rdX, double rdY)
        {
            double a = 0;
            double dX = 1E-5 * (rdX - RDOriginX);
            double dY = 1E-5 * (rdY - RDOriginY);

            for (int i = 0; i < 11; i++)
                a = a + (lat[i][2] * Math.Pow(dX, lat[i][0]) * Math.Pow(dY, lat[i][1]));

            return Math.Round(GpsOriginLat + (a / 3600), 9);
        }

        public double toLon(double rdX, double rdY)
        {
            double a = 0;
            double dX = 1E-5 * (rdX - RDOriginX);
            double dY = 1E-5 * (rdY - RDOriginY);

            for (int i = 0; i < 12; i++)
                a = a + (lon[i][2] * Math.Pow(dX, lon[i][0]) * Math.Pow(dY, lon[i][1]));

            return Math.Round(GpsOriginLon + (a / 3600), 9);
        }

        public static string ConvertDeg2DegMinutes(double a)
        {
            double absdegrees = Math.Abs(a);
            int mydegrees = (int)absdegrees;
            double remaining = absdegrees - 1 * mydegrees;
            double myminutes = 60 * remaining;

            myminutes = Math.Round(1E3 * myminutes) / 1E3;

            return mydegrees + " " + myminutes.ToString("00.000");
        }

        public static void Project(double lon0, double lat0, double angle, int meters, out double lon, out double lat)
        {
            double dx = Math.Sin(Deg2Rad(angle)) * meters;
            double dy = Math.Cos(Deg2Rad(angle)) * meters;

            Project(lon0, lat0, dx, dy, out lon, out lat);
        }

        public static void Project(double lon0, double lat0, double dx, double dy, out double lon, out double lat)
        {
            lat = lat0 + (180 / Math.PI) * (dy / 6378137);
            lon = lon0 + (180 / Math.PI) * (dx / 6378137) / Math.Cos(Deg2Rad(lat0));
        }

        private static double Deg2Rad(double angle)
        {
            return angle * Math.PI / 180.0;
        }
    }
}
