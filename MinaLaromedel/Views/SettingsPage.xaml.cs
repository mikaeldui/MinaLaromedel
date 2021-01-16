using MinaLaromedel.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MinaLaromedel.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public string Username => SettingsService.Username;

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //await ApplicationData.Current.LocalFolder.DeleteAsync();
            //ApplicationData.Current.LocalSettings.Values.Clear();

            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(LoginPage), null);
        }
    }
}
