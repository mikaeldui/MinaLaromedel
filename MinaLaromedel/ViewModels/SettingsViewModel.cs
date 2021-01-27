using GalaSoft.MvvmLight;
using MinaLaromedel.Services;
using MinaLaromedel.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MinaLaromedel.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private long? _cacheSize;

        public SettingsViewModel()
        {
            ClearCache = new RelayCommand(async () => { await ApplicationData.Current.LocalCacheFolder.DeleteAsync(); await RefreshCacheSizeAsync(); });

            if (!IsInDesignModeStatic)
            {
                _ = RefreshCacheSizeAsync();
            }
        }

        public string Username => (new PasswordVault()).FindAllByResource("Hermods Novo").FirstOrDefault()?.UserName;

        public long? CacheSize
        {
            get => _cacheSize;
            set
            {
                if (_cacheSize != value)
                {
                    _cacheSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand Logout { get; } = new RelayCommand(async () =>
        {
            await ApplicationData.Current.LocalFolder.DeleteAsync();
            ApplicationData.Current.LocalSettings.Values.Clear();

            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(LoginPage), null);
        });

        public ICommand ClearCache { get; }

        public async Task RefreshCacheSizeAsync()
        {
            // Query all files in the folder. Make sure to add the CommonFileQuery
            // So that it goes through all sub-folders as well
            var folders = ApplicationData.Current.LocalCacheFolder.CreateFileQuery(CommonFileQuery.OrderByName);

            // Await the query, then for each file create a new Task which gets the size
            var fileSizeTasks = (await folders.GetFilesAsync()).Select(async file => (await file.GetBasicPropertiesAsync()).Size);

            // Wait for all of these tasks to complete. WhenAll thankfully returns each result
            // as a whole list
            var sizes = await Task.WhenAll(fileSizeTasks);

            // Sum all of them up. You have to convert it to a long because Sum does not accept ulong.
            var folderSize = sizes.Sum(l => (long)l);

            await UIThread.RunAsync(() => CacheSize = folderSize);
        }
    }
}
