using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace GpSConverter
{
    public class GeoEventArgs : EventArgs
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public GeoEventArgs(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }

    public delegate void GeoLocationChangedEventHandler(object sender, GeoEventArgs e);

    public class GeoLocation
    {
        private GeolocationAccessStatus mAccessStatus;
        private Timer mTimer;

        public GeoLocation(GeoLocationChangedEventHandler handler)
        {
            Changed = handler;
            Init();            
        }

        public event GeoLocationChangedEventHandler Changed;

        public static void ResetMapPositions(MapControl mapControl, double lat, double lon)
        {
            BasicGeoposition pos = new BasicGeoposition() { Latitude = lat, Longitude = lon };

            Geopoint point = new Geopoint(pos);

            MapIcon poi = new MapIcon { Location = point, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = "You are here", ZIndex = 0 };

            mapControl.MapElements.Clear();
            mapControl.MapElements.Add(poi);
            mapControl.Center = point;
            mapControl.ZoomLevel = 10;
            mapControl.LandmarksVisible = false;
        }

        public static void ZoomToPosition(MapControl mapControl, double lat, double lon)
        {
            BasicGeoposition pos = new BasicGeoposition() { Latitude = lat, Longitude = lon };

            Geopoint point = new Geopoint(pos);

            mapControl.Center = point;
            mapControl.ZoomLevel = 10;
            mapControl.LandmarksVisible = false;
        }


        public static void SetMapPosition(MapControl mapControl, double lat, double lon, string text, double zoomLevel = 10)
        {
            BasicGeoposition pos = new BasicGeoposition() { Latitude = lat, Longitude = lon };

            Geopoint point = new Geopoint(pos);

            MapIcon poi = new MapIcon { Location = point, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = text, ZIndex = 1 };

            mapControl.MapElements.Add(poi);
            mapControl.Center = point;
            mapControl.ZoomLevel = zoomLevel;
            mapControl.LandmarksVisible = false;
        }

        private async void Init()
        {
            mAccessStatus = await Geolocator.RequestAccessAsync();
            mTimer = new Timer(OnGPSTimer, this, 2000, 5000);
        }

        private async void OnGPSTimer(object state)
        {
            GeoLocation p = state as GeoLocation;

            switch (p.mAccessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };

                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    double dlat = pos.Coordinate.Point.Position.Latitude;
                    double dlon = pos.Coordinate.Point.Position.Longitude;

                    GeoEventArgs args = new GeoEventArgs(dlat, dlon);

                    Changed(this, args);
                    break;
            }
        }
    }
}
