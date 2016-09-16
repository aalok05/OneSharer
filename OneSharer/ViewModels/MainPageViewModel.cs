using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using OneSharer.Services;

namespace OneSharer.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
         
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
               
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
               
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
    
        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public async static Task<string> GetFbName()
        {
            var IsFbAuth = await AuthServcie.CheckFbAuthAsync();

            if (IsFbAuth)
            {
                var info = await UserInfoUtility.GetFacebookUserInfo();
                return info;
            }
            else
            {
                return "";

            }
        }
        public async static Task<string> GetTwitterName()
        {
            var IsTwAuth = await AuthServcie.CheckTwitterAuthAsync();
            if (IsTwAuth)
            {
                var info = await UserInfoUtility.GetTwitterUserInfo();
                return info;
            }
            else
            {
                return "";

            }
        }
    }
}

