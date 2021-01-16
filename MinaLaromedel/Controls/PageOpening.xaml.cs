using MinaLaromedel.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MinaLaromedel.Controls
{
    public sealed partial class PageOpening : UserControl
    {
        public PageOpening()
        {
            this.InitializeComponent();
        }

        public ImageSource LeftImageSource
        {
            get { return (ImageSource)GetValue(LeftImageSourceProperty); }
            set { SetValue(LeftImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for imageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftImageSourceProperty =
            DependencyProperty.Register("LeftImageSource", typeof(ImageSource), typeof(PageOpening), new PropertyMetadata(0));

        public ImageSource RightImageSource
        {
            get { return (ImageSource)GetValue(RightImageSourceProperty); }
            set { SetValue(RightImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightImageSourceProperty =
            DependencyProperty.Register("RightImageSource", typeof(ImageSource), typeof(PageOpening), new PropertyMetadata(0));


        #region Mouse Panning

        Pointer pointer;
        PointerPoint scrollMousePoint;
        double hOff = 1;
        double vOff = 1;

        private void MainScrollviewer_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            pointer = e.Pointer;
            scrollMousePoint = e.GetCurrentPoint(scrollViewer);
            hOff = scrollViewer.HorizontalOffset;
            vOff = scrollViewer.VerticalOffset;
            scrollViewer.CapturePointer(pointer);
        }

        private void MainScrollviewer_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ReleasePointerCaptures();
        }

        private void MainScrollviewer_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer.PointerCaptures != null && scrollViewer.PointerCaptures.Count > 0)
            {
                scrollViewer.ChangeView(hOff + (scrollMousePoint.Position.X - e.GetCurrentPoint(scrollViewer).Position.X),
                                        vOff + (scrollMousePoint.Position.Y - e.GetCurrentPoint(scrollViewer).Position.Y), null);
            }
        }

        private void ScrollViewerMain_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        private void ScrollViewerMain_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        #endregion Mouse Panning
    }
}
