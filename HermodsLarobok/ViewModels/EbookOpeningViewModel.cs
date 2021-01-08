﻿using GalaSoft.MvvmLight;
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
                _leftPagePath = value;
                RaisePropertyChanged();
            }
        }

        public string RightPagePath
        {
            get { return _rightPagePath; }
            set
            {
                _rightPagePath = value;
                RaisePropertyChanged();
            }
        }
    }
}
