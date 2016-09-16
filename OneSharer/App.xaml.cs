using System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using OneSharer.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Mvvm;
using Template10.Common;
using System.Linq;
using Windows.UI.Xaml.Data;
using OneSharer.Views;
using Windows.UI.Xaml.Controls;

namespace OneSharer
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
            //SplashFactory = (e) => new ExtendedSplashScreen(e);

            #region App settings
            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            #endregion
        }
   
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                Window.Current.Content = rootFrame;
            }

          
                if (rootFrame.Content == null)
                {
                    ExtendedSplashScreen extendedSplash = new ExtendedSplashScreen(args.SplashScreen);
                    rootFrame.Content = extendedSplash;

                }
                Window.Current.Activate();
            
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            await Task.CompletedTask;
            //ExtendedSplashScreen extendedSplash = new ExtendedSplashScreen(args.SplashScreen);
            //rootFrame.Content = extendedSplash;
            //await NavigationService.NavigateAsync(typeof(ExtendedSplashScreen));
        }
    }
}

