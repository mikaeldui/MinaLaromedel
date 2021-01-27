using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinaLaromedel.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            //}
            //else
            //{
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            SimpleIoc.Default.Register<EbooksViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public EbooksViewModel Ebooks => SimpleIoc.Default.GetInstance<EbooksViewModel>();
        public SettingsViewModel Settings => SimpleIoc.Default.GetInstance<SettingsViewModel>();
    }
}
