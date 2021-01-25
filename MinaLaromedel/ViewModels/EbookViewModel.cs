using GalaSoft.MvvmLight;
using MinaLaromedel.Services;
using MinaLaromedel.Storage;
using Hermods.Novo;
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
using Windows.UI.Input.Inking;
using GalaSoft.MvvmLight.Messaging;
using MinaLaromedel.Messages;
using MinaLaromedel.Tiles;

namespace MinaLaromedel.ViewModels
{
    public class EbookViewModel : ViewModelBase
    {
        public HermodsNovoEbook _ebook;
        private bool _isDownloaded = false;
        private bool _isDownloading = false;
        private bool _isDownloadable = false;
        private string _frontPagePath;
        private bool _isPinnable = false;
        private string _downloadStatus;

        public EbookViewModel(HermodsNovoEbook ebook) 
        {
            _ebook = ebook;

            Download = new RelayCommand(async () =>
            {
                IsDownloadable = false;
                IsDownloading = true;

                Messenger.Default.Register<DownloadStatusMessage>(this, _ebook.Isbn, msg =>
                {
                    DownloadStatus = $"{msg.Done} av {msg.Total} sidor nerladdade";
                });

                await EbookService.DownloadEbookAsync(_ebook);

                IsDownloading = false;
                IsDownloaded = true;
                IsPinnable = true;

                FrontPagePath = await PageStorage.GetPagePathAsync(_ebook, 1);
            });

            Pin = new RelayCommand(async () =>
            {
                IsPinnable = !(await EbookTile.RequestCreateAsync(_ebook));
            });

            _ = _asyncInit();            
        }

        private async Task _asyncInit()
        {
            if (!await PageStorage.EbookExistsAsync(_ebook))
            {
                if (NetworkInformation.GetInternetConnectionProfile() != null)
                    IsDownloadable = true;
            }
            else
            {
                IsDownloaded = true;
                IsPinnable = await Task.Run(() => !EbookTile.Exists(_ebook.Isbn));

                try
                {
                    FrontPagePath = await PageStorage.GetPagePathAsync(_ebook, 1);
                }
                catch { }
            }
        }

        public string Title => _ebook.Title;

        public string Isbn => _ebook.Isbn;

        #region Download

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

        public string DownloadStatus
        {
            get => _downloadStatus;
            set
            {
                if (_downloadStatus != value)
                {
                    _downloadStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand Download { get; }

        #endregion Download

        #region Pin

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

        public ICommand Pin { get; }

        #endregion Pin

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
    }
}
