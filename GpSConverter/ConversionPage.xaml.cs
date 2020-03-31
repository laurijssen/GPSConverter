using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GpSConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversionPage : Page
    {
        private MessageDialog mMsgDlg = new MessageDialog("");
        private GeoLocation loc;

        private bool mLocSet = false;

        public ConversionPage()
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
                await disp.RunAsync(CoreDispatcherPriority.Normal, () => { GeoLocation.ResetMapPositions(mapControl, dlat, dlon); });

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    tbCurCoords.Text = GPSConverter.ConvertDeg2DegMinutes(dlat) + "," + GPSConverter.ConvertDeg2DegMinutes(dlon);
                    IsEnabled = true;
                });
                mLocSet = true;
            }
        }

        private async void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            string coords = tbCoords.Text;

            if (cbCoords.SelectedIndex == 0)
            {
                string[] fields = coords.Split(',');

                string slat = fields[0];
                string slon = fields[1];



            }
            else if (cbCoords.SelectedIndex == 1)
            {
                string[] fields = coords.Split(',');

                if (fields.Length == 2)
                {
                    string[] latFields = fields[0].Split(' ');
                    string[] lonFields = fields[1].Split(' ');

                    int lon, lat;

                    if (int.TryParse(fields[0], out lat) && int.TryParse(fields[1], out lon))
                    {
                        GPSConverter c = new GPSConverter();

                        double newLat = c.toLat(lat, lon);
                        double newLon = c.toLon(lat, lon);

                        mMsgDlg.Content = (newLat < 0 ? "S" : "N") + GPSConverter.ConvertDeg2DegMinutes(newLat) +
                                       (newLon < 0 ? " W" : " E") + GPSConverter.ConvertDeg2DegMinutes(newLon);
                        await mMsgDlg.ShowAsync();

                        GeoLocation.SetMapPosition(mapControl, newLat, newLon, mMsgDlg.Content, 15);
                    }
                    else
                    {
                        mMsgDlg.Content = "Incorrect Dutch Grid format. It must be of the form xxxxx,yyyyyy";
                        await mMsgDlg.ShowAsync();
                    }
                }
                else
                {
                    mMsgDlg.Content = "Incorrect Dutch Grid format. It must be of the form xxxxx,yyyyyy";
                    await mMsgDlg.ShowAsync();
                }
            }
            else
            {

                mMsgDlg.Content = coords;
                await mMsgDlg.ShowAsync();
            }
        }

        private void tbCoords_GotFocus(object sender, RoutedEventArgs e)
        {
            tbCoords.Text = "";
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            tbCoords.Text = tbCurCoords.Text;
        }
    }
}
