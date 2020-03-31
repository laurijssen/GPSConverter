using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GpSConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        public MapPage()
        {
            this.InitializeComponent();
        }

        private void mainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            mapControl.Width = mainGrid.ActualWidth - 20;
            mapControl.Height = mainGrid.ActualHeight - 30;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter as Tuple<double, double>;

            if (parameter != null)
            {
                double lat = parameter.Item1;
                double lon = parameter.Item2;

                BasicGeoposition pos = new BasicGeoposition() { Latitude = lat, Longitude = lon };

                Geopoint point = new Geopoint(pos);

                MapIcon poi = new MapIcon { Location = point, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = "You are here", ZIndex = 0 };

                mapControl.MapElements.Add(poi);
                mapControl.Center = point;
                mapControl.ZoomLevel = 10;
                mapControl.LandmarksVisible = false;
            }
        }
    }
}
