using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

using System.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GpSConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CrossBearingPage : Page
    {
        private MessageDialog mMsgDlg = new MessageDialog("");

        GeoLocation loc;

        private bool mLocSet = false;

        private List<BasicGeoposition> mPoints = new List<BasicGeoposition>();

        public CrossBearingPage()
        {
            this.InitializeComponent();

            loc = new GeoLocation(ChangedEventHandler);

            IsEnabled = false;
        }

        async void ChangedEventHandler(object sender, GeoEventArgs e)
        {
            if (!mLocSet)
            {
                CoreDispatcher disp = CoreApplication.MainView.CoreWindow.Dispatcher;

                double dlat = e.Lat;
                double dlon = e.Lon;

                await disp.RunAsync(CoreDispatcherPriority.Normal, () => {
                    rbNorth.IsChecked = dlat > 0;
                    rbSouth.IsChecked = dlat < 0;
                    rbEast.IsChecked = dlon > 0;
                    rbWest.IsChecked = dlon < 0;
                    tbWait.Visibility = Visibility.Collapsed;
                });
                await disp.RunAsync(CoreDispatcherPriority.Normal, () => { GeoLocation.ZoomToPosition(mapControl, dlat, dlon); });

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    tbCurCoords.Text = GPSConverter.ConvertDeg2DegMinutes(dlat) + "," + GPSConverter.ConvertDeg2DegMinutes(dlon);
                });

                await disp.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    IsEnabled = true;
                });

                mLocSet = true;
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            double lat, lon;

            int n1, n2, n3;
            int e1, e2, e3;

            if (int.TryParse(tbCoordsN1.Text, out n1) && int.TryParse(tbCoordsN2.Text, out n2) && int.TryParse(tbCoordsN3.Text, out n3) &&
                int.TryParse(tbCoordsE1.Text, out e1) && int.TryParse(tbCoordsE2.Text, out e2) && int.TryParse(tbCoordsE3.Text, out e3))
            {
                double.TryParse(n2 + "." + n3, out lat);
                lat /= 60.0;
                lat = n1 + lat;

                double.TryParse(e2 + "." + e3, out lon);
                lon /= 60.0;
                lon = e1 + lon;

                if ((bool)rbSouth.IsChecked)
                    lat = -lat;

                if ((bool)rbWest.IsChecked)
                    lon = -lon;

                BasicGeoposition pos = new BasicGeoposition { Latitude = lat, Longitude = lon };

                mPoints.Add(pos);

                GeoLocation.SetMapPosition(mapControl, lat, lon, "" + mPoints.Count, mapControl.ZoomLevel);

                if (mPoints.Count >= 2)
                {
                    List<BasicGeoposition> l = new List<BasicGeoposition>() { mPoints[0], mPoints[1] };
                    var shape = new MapPolyline
                    {
                        StrokeThickness = 3,
                        StrokeColor = Colors.Green,
                        StrokeDashed = true,
                        ZIndex = 1,
                        Path = new Geopath(l)
                    };
                    mapControl.MapElements.Add(shape);
                }
                if (mPoints.Count >= 4)
                {
                    List<BasicGeoposition> l = new List<BasicGeoposition>() { mPoints[2], mPoints[3] };

                    var shape = new MapPolyline
                    {
                        StrokeThickness = 3,
                        StrokeColor = Colors.Red,
                        StrokeDashed = true,
                        ZIndex = 1,
                        Path = new Geopath(l)                        
                    };
                    mapControl.MapElements.Add(shape);

                    if (getLineIntersection(mPoints[0].Longitude, mPoints[0].Latitude, mPoints[1].Longitude, mPoints[1].Latitude,
                                              mPoints[2].Longitude, mPoints[2].Latitude, mPoints[3].Longitude, mPoints[3].Latitude,
                                              out lon, out lat))
                    {
                        string text = "";

                        if ((bool)rbNorth.IsChecked)
                            text = "N ";

                        text += tbCoordsN1.Text + " " + tbCoordsN2.Text + "." + tbCoordsN3.Text + " ";

                        if ((bool)rbWest.IsChecked)
                            text += "W ";
                        else
                            text += "E ";

                        text += tbCoordsE1.Text + " " + tbCoordsE2.Text + "." + tbCoordsE3.Text;

                        GeoLocation.SetMapPosition(mapControl, lat, lon, text, mapControl.ZoomLevel);

                        SetGeoPosition(lat, lon);
                    }
                }
            }
            else
            {
                mMsgDlg.Content = "Enter all numeric values in the boxes";
                await mMsgDlg.ShowAsync();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            mapControl.MapElements.Clear();
            mPoints.Clear();
        }

        private void mapControl_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            if (!mapControl.IsEnabled) return;

            Geopoint pt = args.Location;

            double lat = pt.Position.Latitude;
            double lon = pt.Position.Longitude;

            SetGeoPosition(lat, lon);   
        }

        private void SetGeoPosition(double lat, double lon)
        {
            if (lat < 0)
                rbSouth.IsChecked = true;
            else
                rbNorth.IsChecked = true;

            if (lon < 0)
                rbWest.IsChecked = true;
            else
                rbEast.IsChecked = true;

            lat = Math.Abs(lat);
            lon = Math.Abs(lon);

            int i = (int)lat;
            tbCoordsN1.Text = "" + i;
            lat = (lat - i) * 60;
            i = (int)lat;
            tbCoordsN2.Text = "" + i;
            tbCoordsN3.Text = "" + (int)((lat - i) * 1000);

            i = (int)lon;
            tbCoordsE1.Text = "" + i;
            lon = (lon - i) * 60;
            i = (int)lon;
            tbCoordsE2.Text = "" + i;
            tbCoordsE3.Text = "" + (int)((lon - i) * 1000);
        }

        private bool getLineIntersection(double p0_x, double p0_y, double p1_x, double p1_y,
            double p2_x, double p2_y, double p3_x, double p3_y, out double i_x, out double i_y)
        {
            double s1_x, s1_y, s2_x, s2_y;
            double s, t;

            i_x = 0;
            i_y = 0;

            s1_x = p1_x - p0_x; s1_y = p1_y - p0_y;
            s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;            

            s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
            t = (s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                // Collision detected
                i_x = p0_x + (t * s1_x);
                i_y = p0_y + (t * s1_y);
                return true;
            }

            return false; // No collision
        }
    }
}
