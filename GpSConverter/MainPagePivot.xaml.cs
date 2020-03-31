using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GpSConverter
{
    public sealed partial class MainPagePivot : Page
    {        
        private ContentDialog mCntDlg = new ContentDialog();
        
        public MainPagePivot()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            mCntDlg.PrimaryButtonText = "Close";
        }

        private void tbStackCount_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string s = tbStackCount.Text;

            int A0 = 0;
            int A1 = 0;
            int A0Total = 0;
            int A1Total = 0;

            foreach (char c in s)
            {
                if (!char.IsLetterOrDigit(c))
                    continue;

                int value;

                value = StackLetterToInt(char.ToUpper(c) - 'A');
                A0 = StackLetterToInt(A0 + value);

                value = StackLetterToInt(char.ToUpper(c) - 'A' + 1);
                A1 = StackLetterToInt(A1 + value);

                value = char.ToUpper(c) - 'A';
                A0Total += value;

                value = char.ToUpper(c) - 'A' + 1;
                A1Total += value;
            }

            lblA0.Text = A0 + " " + A0Total;
            lblA1.Text = A1 + " " + A1Total;
        }

        private int StackLetterToInt(int value)
        {
            while (value >= 10)
            {
                int div = value / 10;
                int rem = value % 10;

                value = div + rem;
            }

            return value;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.SuggestedFileName = "New Document";
            picker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteTextAsync(file, tbNotes.Text);
                await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (IInputStream istream = stream.GetInputStreamAt(0))
                    {
                        using (var dataReader = new DataReader(istream))
                        {
                            await dataReader.LoadAsync((uint)stream.Size);

                            string text = dataReader.ReadString((uint)stream.Size);
                            tbNotes.Text = text;
                            dataReader.DetachStream();
                        }
                    }
                }
            }
        }

        private async void ShowContentDialog(string asset)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.ZoomMode = ZoomMode.Enabled;
            scrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
            scrollViewer.VerticalScrollMode = ScrollMode.Enabled;

            Image img = new Image();
            img.Source = new BitmapImage(new Uri(asset));

            scrollViewer.Content = img;

            mCntDlg.Content = scrollViewer;

            await mCntDlg.ShowAsync();
        }

        private void Notes_Loaded(object sender, RoutedEventArgs e)
        {
            tbNotes.Width = gridNotes.ActualWidth - 50;
        }

        private void lstCoords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lstCoords.SelectedIndex)
            {
                case 0:
                    Frame.Navigate(typeof(ProjectionPage));
                    break;

                case 1:
                    Frame.Navigate(typeof(ConversionPage));
                    break;

                case 2:
                    Frame.Navigate(typeof(CrossBearingPage));
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            //if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
                {
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        a.Handled = true;
                    }
                };
            }
        }

        private void lstPuzzles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lstPuzzles.SelectedIndex)
            {
                case 0:
                    mCntDlg.Title = "Navy flags";

                    ShowContentDialog("ms-appx:///Assets/navyflags.jpg");
                    break;

                case 1:
                    mCntDlg.Title = "Dancing man";

                    ShowContentDialog("ms-appx:///Assets/dancingmen.jpg");
                    break;

                case 2:
                    mCntDlg.Title = "Morse";

                    ShowContentDialog("ms-appx:///Assets/morsecode.jpg");
                    break;

                case 3:
                    mCntDlg.Title = "Braille";

                    ShowContentDialog("ms-appx:///Assets/braille.jpg");
                    break;

                case 4:
                    mCntDlg.Title = "Sign language";

                    ShowContentDialog("ms-appx:///Assets/signlanguage.jpg");
                    break;

                case 5:
                    mCntDlg.Title = "Flag code";

                    ShowContentDialog("ms-appx:///Assets/flagcode.jpg");
                    break;

                case 6:
                    mCntDlg.Title = "Periodic Table";
                    ShowContentDialog("ms-appx:///Assets/periodictable.jpg");
                    break;
            }
        }

        private void lstAlphabets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lstAlphabets.SelectedIndex)
            {
                case 0:
                    mCntDlg.Title = "Latin alphabet";

                    ShowContentDialog("ms-appx:///Assets/latin.jpg");
                    break;

                case 1:
                    mCntDlg.Title = "Greek alphabet";

                    ShowContentDialog("ms-appx:///Assets/greek.jpg");
                    break;

                case 2:
                    mCntDlg.Title = "Hebrew alphabet";

                    ShowContentDialog("ms-appx:///Assets/hebrew.jpg");
                    break;

                case 3:
                    mCntDlg.Title = "Old Cyrillic alphabet";

                    ShowContentDialog("ms-appx:///Assets/oldcyrillic.jpg");
                    break;
            }
        }

        private void tbFrom_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            int rot = cbRot.SelectedIndex + 1;
            tbTo.Text = RotX(tbFrom.Text, rot);
        }

        private void tbVigenereText_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbVigenereKey.Text))
            {
                tbVigenereResult.Text = Vigenere.Decipher(tbVigenereText.Text, tbVigenereKey.Text);
            }
        }
        
        private void cbRot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTo != null)
            {
                tbTo.Text = "";
                int rot = cbRot.SelectedIndex + 1;
                tbTo.Text = RotX(tbFrom.Text, rot);
            }
        }

        private string RotX(string text, int rot)
        {
            string outText = "";

            foreach (char c in text)
            {
                if (char.IsUpper(c))
                {
                    int x = c + (rot % 26);

                    if (x > (int)'Z')
                        x -= 26;

                    outText += (char)x;
                }
                else if (char.IsLower(c))
                {
                    int x = c + (rot % 26);

                    if (x > (int)'z')
                        x -= 26;

                    outText += (char)x;
                }
                else
                {
                    outText += c;
                }
            }
            return outText;
        }

        private async void btnAddLetter_Click(object sender, RoutedEventArgs e)
        {
            const int START_ROWS = 6;

            if (gridNotes.RowDefinitions.Count - START_ROWS >= 13)
            {
                MessageDialog mMsgDlg = new MessageDialog("Cannot add more letters");
                await mMsgDlg.ShowAsync();
                return;
            }

            TextBlock block = new TextBlock();
            TextBox box = new TextBox();

            block.VerticalAlignment = VerticalAlignment.Center;
            block.Text = (char)('A' + (gridNotes.RowDefinitions.Count - START_ROWS)) + " =";

            gridNotes.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(31) });

            Grid.SetRow(block, gridNotes.RowDefinitions.Count - 1);

            Grid.SetRow(box, gridNotes.RowDefinitions.Count - 1);
            Grid.SetColumn(box, 2);

            gridNotes.Children.Add(block);
            gridNotes.Children.Add(box);
        }
    }
}
