using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GpSConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectionPage : Page
    {
        private MessageDialog mMsgDlg = new MessageDialog("");

        private GeoLocation loc;

        private bool mLocSet = false;

        public ProjectionPage()
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

                await disp.RunAsync(CoreDispatcherPriority.Normal, () => { rbNorth.IsChecked = dlat > 0;
                            rbSouth.IsChecked = dlat < 0;
                            rbEast.IsChecked = dlon > 0;
                            rbWest.IsChecked = dlon < 0;
                            tbWait.Visibility = Visibility.Collapsed;
                });
                await disp.RunAsync(CoreDispatcherPriority.Normal, () => { ResetMapPositions(dlat, dlon); });

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    tbCurCoords.Text = GPSConverter.ConvertDeg2DegMinutes(dlat) + "," + GPSConverter.ConvertDeg2DegMinutes(dlon);
                    IsEnabled = true;
                });
                mLocSet = true;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            tbCoords.Text = tbCurCoords.Text;
        }

        private async void btnProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };

                Geoposition pos = await geolocator.GetGeopositionAsync();

                double dlat = pos.Coordinate.Point.Position.Latitude;
                double dlon = pos.Coordinate.Point.Position.Longitude;

                ResetMapPositions(dlat, dlon);

                if (cbCoords.SelectedIndex == 0)
                {
                    string[] fields = tbCoords.Text.Split(',');

                    if (fields.Length == 2)
                    {
                        string[] latCoords = fields[0].Trim(' ').Split(' ');
                        string[] lonCoords = fields[1].Trim(' ').Split(' ');

                        double lat, lon;

                        lat = double.Parse(latCoords[0]) + (double.Parse(latCoords[1]) / 60);
                        lon = double.Parse(lonCoords[0]) + (double.Parse(lonCoords[1]) / 60);

                        double newLon, newLat;

                        int angle, meters;

                        if (!int.TryParse(tbProjectAngle.Text, out angle))
                        {
                            mMsgDlg.Content = "Please enter an angle in the angle box on the left";
                            await mMsgDlg.ShowAsync();
                            return;
                        }
                        if (!int.TryParse(tbProjectMeters.Text, out meters))
                        {
                            mMsgDlg.Content = "Please enter the distance in the distance box on the right";
                            await mMsgDlg.ShowAsync();
                            return;
                        }

                        if (cbMeters.SelectedIndex == 1)
                            meters *= 1000;
                        else if (cbMeters.SelectedIndex == 2)
                            meters = (int)Math.Round(meters * 1609.344);

                        GPSConverter.Project(lon, lat, angle, meters, out newLon, out newLat);

                        mMsgDlg.Content = newLat.ToString("00.00000") + " " + newLon.ToString("000.00000") + "\n" +
                                       GPSConverter.ConvertDeg2DegMinutes(newLat) + " " + GPSConverter.ConvertDeg2DegMinutes(newLon);
                        await mMsgDlg.ShowAsync();
                        SetMapPosition(newLat, newLon, "Projected", 15);
                    }
                    else
                    {
                        mMsgDlg.Content = "Latitude and longitude must be separated by a comma";
                        await mMsgDlg.ShowAsync();
                    }
                }
                else if (cbCoords.SelectedIndex == 1)
                {
                    string[] fields = tbCoords.Text.Split(',');

                    if (fields.Length == 2)
                    {
                        int angle, meters;

                        if (!int.TryParse(tbProjectAngle.Text, out angle))
                        {
                            mMsgDlg.Content = "Please enter an angle in the angle box on the left";
                            await mMsgDlg.ShowAsync();
                            return;
                        }
                        if (!int.TryParse(tbProjectMeters.Text, out meters))
                        {
                            mMsgDlg.Content = "Please enter the distance in the distance box on the right";
                            await mMsgDlg.ShowAsync();
                            return;
                        }

                        double lat = double.Parse(fields[0].Trim());
                        double lon = double.Parse(fields[1].Trim());

                        if (cbMeters.SelectedIndex == 1)
                            meters *= 1000;

                        double newLat, newLon;

                        GPSConverter.Project(lon, lat, angle, meters, out newLon, out newLat);

                        mMsgDlg.Content = newLat.ToString("00.00000") + " " + newLon.ToString("000.00000") + "\n" +
                                       GPSConverter.ConvertDeg2DegMinutes(newLat) + " " + GPSConverter.ConvertDeg2DegMinutes(newLon);
                        await mMsgDlg.ShowAsync();
                        SetMapPosition(newLat, newLon, "Projected", 15);
                    }
                    else
                    {
                        mMsgDlg.Content = "Latitude and longitude must be separated by a comma";
                        await mMsgDlg.ShowAsync();
                    }
                }
                else
                {
                    mMsgDlg.Content = "Only Degrees and Degrees Minutes projection is implemented.";
                    await mMsgDlg.ShowAsync();
                }
            }
            finally
            {
                tbProjectAngle.Text = "Deg";
                tbProjectMeters.Text = "Dst";
            }
        }

        private void tbProjectAngle_GotFocus(object sender, RoutedEventArgs e)
        {
            tbProjectAngle.Text = "";
        }

        private void tbProjectMeters_GotFocus(object sender, RoutedEventArgs e)
        {
            tbProjectMeters.Text = "";
        }

        private void ResetMapPositions(double lat, double lon)
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

        private void SetMapPosition(double lat, double lon, string text, int zoomLevel = 10)
        {
            BasicGeoposition pos = new BasicGeoposition() { Latitude = lat, Longitude = lon };

            Geopoint point = new Geopoint(pos);

            MapIcon poi = new MapIcon { Location = point, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = text, ZIndex = 0 };

            mapControl.MapElements.Add(poi);
            mapControl.Center = point;
            mapControl.ZoomLevel = zoomLevel;
            mapControl.LandmarksVisible = false;
        }

        private void Coordinates_Loaded(object sender, RoutedEventArgs e)
        {
            mapControl.Width = gridCoordinates.ActualWidth - 20;
            mapControl.Height = gridCoordinates.ActualHeight - 30;
        }

        private void tbCoords_GotFocus(object sender, RoutedEventArgs e)
        {
            tbCoords.Text = "";
        }
    }
}
