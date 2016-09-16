using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneSharer.Views
{

    public sealed partial class ExtendedSplashScreen : Page
    {
        private Rect _splashImageBounds;

        public ExtendedSplashScreen(SplashScreen splashScreen)
        {
            InitializeComponent();

            if (splashScreen != null)
            {
                _splashImageBounds = splashScreen.ImageLocation;
            }
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the main page
            MainPage page = new MainPage(_splashImageBounds);

            // ... and navigate to the Main Page
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                Window.Current.Content = rootFrame = new Frame();
            }

            rootFrame.Content = page;
        }
    }
}
