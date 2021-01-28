using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.WindowManagement;

namespace MinaLaromedel.Helpers
{
    public class AppWindowFullScreenHelper
    {
        private AppWindow _appWindow;

        public AppWindowFullScreenHelper(AppWindow appWindow)
        {
            _appWindow = appWindow;
        }

        public bool TryEnterFullScreen()
        {
            var configuration = _appWindow.Presenter.GetConfiguration();
            if (_appWindow.Presenter.RequestPresentation(AppWindowPresentationKind.FullScreen))
            {
                FullScreenEntered?.Invoke(this, null);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await UIThread.RunAsync(() =>
                    {
                        _appWindow.Changed += AppWindow_Changed;
                    });
                });

                return true;
            }

            return false;
        }

        public bool TryExitFullScreen()
        {
            if (_appWindow.Presenter.RequestPresentation(AppWindowPresentationKind.Default))
            {
                FullScreenExited?.Invoke(this, null);
                _appWindow.Changed -= AppWindow_Changed;

                return true;
            }

            return false;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange)
                TryExitFullScreen();
        }

        public event EventHandler FullScreenEntered;
        public event EventHandler FullScreenExited;
    }
}
