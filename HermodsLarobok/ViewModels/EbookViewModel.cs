using GalaSoft.MvvmLight;
using HermodsLarobok.Clients;
using HermodsLarobok.Models;
using HermodsLarobok.Services;
using HermodsLarobok.Storage;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking.Connectivity;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace HermodsLarobok.ViewModels
{
    public class EbookViewModel : ViewModelBase
    {
        public Ebook _ebook;
        private bool _isDownloaded = false;
        private bool _isDownloading = false;
        private bool _isDownloadable = false;
        private string _frontPagePath;
        private bool _isPinnable = false;

        public EbookViewModel(Ebook ebook) 
        {
            _ebook = ebook;

            Download = new RelayCommand(async () =>
            {
                IsDownloadable = false;
                IsDownloading = true;

                await EbookDownloadService.DownloadEbookAsync(_ebook);

                IsDownloading = false;
                IsDownloaded = true;

                NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;

                FrontPagePath = await PageStorage.GetPagePathAsync(_ebook, 1);
            });

            Pin = new RelayCommand(async () =>
            {
                // Provide all the required info in arguments so that when user
                // clicks your tile, you can navigate them to the correct content
                string arguments = "action=viewEbook&isbn=" + _ebook.Isbn;

                // Initialize the tile with required arguments
                SecondaryTile tile = new SecondaryTile(
                    "isbn-" + _ebook.Isbn,
                    _ebook.Title,
                    arguments,
                    new Uri("ms-appx:///Assets/CityTiles/Square150x150Logo.png"),
                    TileSize.Default);

                if(await tile.RequestCreateAsync())
                {
                    IsPinnable = false;
                }

            });

            AsyncContext.Run(_asyncInit);            
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            if (IsDownloaded) return;
            await _asyncInit();
        }

        private async Task _asyncInit()
        {
            if (!await PageStorage.EbookExistsAsync(_ebook))
            {
                if (NetworkInformation.GetInternetConnectionProfile() != null)
                    IsDownloadable = true;

                NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            }
            else
            {
                IsDownloaded = true;
                try
                {
                    FrontPagePath = await PageStorage.GetPagePathAsync(_ebook, 1);
                }
                catch { }
            }

            IsPinnable = !SecondaryTile.Exists(_ebook.Isbn);
        }

        public string Title => _ebook.Title;

        public string Isbn => _ebook.Isbn;

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                if (_isDownloaded != value)
                {
                    _isDownloaded = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if(_isDownloading != value)
                {
                    _isDownloading = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsDownloadable
        {
            get => _isDownloadable;
            set
            {
                if (_isDownloadable != value)
                {
                    _isDownloadable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsPinnable
        {
            get => _isPinnable;
            set
            {
                if(_isPinnable != value)
                {
                    _isPinnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string FrontPagePath
        {
            get => _frontPagePath;
            set
            {
                if(_frontPagePath != value)
                {
                    _frontPagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand Download { get; }

        public ICommand Pin { get; }
    }
}
