using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MinaLaromedel
{
    public static class UIThread
    {
        public static async Task RunAsync(DispatchedHandler agileCallback) =>
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
    }
}
