using GalaSoft.MvvmLight;
using MinaLaromedel.Collections;
using MinaLaromedel.Managers;
using MinaLaromedel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MinaLaromedel.ViewModels
{
    public class EbooksViewModel : ViewModelBase
    {
        private readonly Lazy<ObservableCollection<EbookViewModel>> _ebooks = 
            new Lazy<ObservableCollection<EbookViewModel>>(() =>
                new ObservableViewModelCollection<EbookViewModel, Ebook>(EbookManager.Ebooks, EbookViewModel.From));

        private bool _canRefresh = false;

        public EbooksViewModel()
        {
            Refresh = new RelayCommand(async () =>
            {
                CanRefresh = false;
                try
                {
                    await EbookManager.RefreshEbooksAsync();
                }
                finally
                {
                    CanRefresh = true;
                }
            });

            if (!IsInDesignModeStatic)
            {
                _ = Task.Run(async () =>
                {
                    await EbookManager.LoadEbooksAsync();

                    if (EbookManager.Ebooks.Count == 0)
                        await EbookManager.RefreshEbooksAsync();

                    await UIThread.RunAsync(() =>
                    {
                        CanRefresh = true;
                    });
                });
            }
        }

        public ObservableCollection<EbookViewModel> Ebooks { get => _ebooks.Value; }

        public ICommand Refresh { get; }

        public bool CanRefresh
        {
            get => _canRefresh;
            set
            {
                if (_canRefresh != value)
                {
                    _canRefresh = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
