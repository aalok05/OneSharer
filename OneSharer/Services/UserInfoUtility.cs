using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneSharer.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using LinqToTwitter;

namespace OneSharer.Services
{
    class UserInfoUtility : AuthServcie
    {
        public static async Task<string> GetFacebookUserInfo()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string access_token = await GetFbTokenAsync();
                    string response = await httpClient.GetStringAsync(new Uri("https://graph.facebook.com/me?fields=name&access_token=" + access_token));
                    if (response.Contains("name"))
                    {
                        var publicProfile = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<FacebookUser>(response));

                        return (publicProfile.name);
                    }

                    else return "";
                }
            }
            catch
            {
                return "";
            }
        }
        public static async Task<string> GetTwitterUserInfo()
        {
            try
            {
                var authorizer = await GetTwitterCredentials();
                var twitterContext = new TwitterContext(authorizer);

             var accounts =
             from acct in twitterContext.Account
             where acct.Type == AccountType.VerifyCredentials
             select acct;

              Account account = accounts.SingleOrDefault();
              User user = account.User;
                if(string.IsNullOrWhiteSpace(user.ScreenNameResponse))
                {
                    return "";
                }
                else
                {
                    return "@"+user.ScreenNameResponse;
                }
                      
            }
            catch
            {
                return "";
            }
        }
               
    }
}
