using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace HermodsLarobok.ViewModels
{
    public class EbookOpeningViewModel : ViewModelBase
    {
        private string _leftPagePath;
        private string _rightPagePath;
        private string _inkPath;

        public EbookOpeningViewModel(string leftPagePath, string rightPagePath, string inkPath = null)
        {
            _leftPagePath = leftPagePath;
            _rightPagePath = rightPagePath;
            _inkPath = inkPath;
        }

        public string LeftPagePath
        {
            get => _leftPagePath;
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
            get => _rightPagePath;
            set
            {
                if (_rightPagePath != value)
                {
                    _rightPagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string InkPath
        {
            get => _inkPath;
            set
            {
                if (_inkPath != value)
                {
                    _inkPath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
