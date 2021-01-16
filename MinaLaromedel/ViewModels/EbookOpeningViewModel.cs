using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media;

namespace MinaLaromedel.ViewModels
{
    public class EbookOpeningViewModel : ViewModelBase
    {
        private string _leftPagePath;
        private string _rightPagePath;

        public EbookOpeningViewModel(string leftPagePath, string rightPagePath)
        {
            _leftPagePath = leftPagePath;
            _rightPagePath = rightPagePath;
        }

        public string LeftPagePath
        {
            get { return _leftPagePath; }
            set
            {
                if (_leftPagePath != value)
                {
                    _leftPagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string RightPagePath
        {
            get { return _rightPagePath; }
            set
            {
                if (_rightPagePath != value)
                {
                    _rightPagePath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
