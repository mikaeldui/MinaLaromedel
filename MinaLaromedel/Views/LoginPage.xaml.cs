using MinaLaromedel.Services;
using Hermods.Novo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Security.Credentials;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MinaLaromedel.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageTextBlock.Text = "";
            LoginFormStackPanel.Opacity = 0.5;
            UsernameTextBox.IsEnabled = PasswordBoxControl.IsEnabled = LoginButton.IsEnabled = false;
            LoginProgressRing.IsActive = true;

            // Login
            PasswordCredential credential = new PasswordCredential("Hermods Novo", UsernameTextBox.Text, PasswordBoxControl.Password);

            if (await EbookService.TryAuthenticateAsync(credential))
            {
                (new PasswordVault()).Add(credential);
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                ErrorMessageTextBlock.Text = "Ogiltigt användarnamn eller lösenord.";
                // On Error
                UsernameTextBox.IsEnabled = PasswordBoxControl.IsEnabled = LoginButton.IsEnabled = true;
                LoginProgressRing.IsActive = false;
                LoginFormStackPanel.Opacity = 1;
            }
        }
    }
}
