using LinqToTwitter;
using Newtonsoft.Json;
using OneSharer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;


namespace OneSharer.Services
{
    class AuthServcie : TokenUtility
    {
        public static async Task<AuthOutput> AuthenticateToFacebookAsync()
        {
            try
            {
                string FacebookURL = "https://www.facebook.com/dialog/oauth?client_id=" +
                   Uri.EscapeDataString("facebook_client_id") + "&redirect_uri=" +
                   Uri.EscapeDataString("facebook_redirect_uri") +
                   "&scope=publish_actions&display=popup&response_type=token";

                Uri StartUri = new Uri(FacebookURL);
                Uri EndUri = new Uri("http://localhost");

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None,
                                                        StartUri,
                                                        EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    var resp = await CallForLonglivedTokenAsync(WebAuthenticationResult.ResponseData);
                    var tokenSaved = await SaveFbTokenAsync(resp);
                    return (new AuthOutput
                    {
                        Output = WebAuthenticationResult.ResponseData.ToString(),
                        InfoTag = "Sucess"
                    });

                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    return (new AuthOutput
                    {
                        Output = WebAuthenticationResult.ResponseErrorDetail.ToString(),
                        InfoTag = "HTTP Error"
                    });

                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    return (new AuthOutput
                    {
                        Output = WebAuthenticationResult.ResponseErrorDetail.ToString(),
                        InfoTag = "UserCancel"
                    });

                }
                else
                {
                    return (new AuthOutput
                    {
                        Output = WebAuthenticationResult.ResponseStatus.ToString(),
                        InfoTag = "Response status"
                    });

                }

             }
            catch (Exception e)
            {
                return (new AuthOutput
                {
                    Output = e.Message,
                    InfoTag = "Exception"
                });

            }
        }

        public static async Task<bool> AuthenticateToTwitterAsync()
        {
            try
            {
                string key = "twitter_key";
                string sec = "twitter_secret";
              
                var authorizer = new UniversalAuthorizer
                {

                    CredentialStore = new InMemoryCredentialStore
                    {
                        ConsumerKey = key,
                        ConsumerSecret = sec                       
                    }
                    };

                authorizer.Callback = "facebook_callback_uri";
                authorizer.ForceLogin = true;
              
                await authorizer.AuthorizeAsync();
               
                if (authorizer.CredentialStore.OAuthToken == null || authorizer.CredentialStore.OAuthTokenSecret == null)
                {
                    return false;
                }
                else
                {
                    var saved = await SaveTwitterTokenAsync(authorizer.CredentialStore.OAuthToken, authorizer.CredentialStore.OAuthTokenSecret, key, sec);
                    return saved;
                }
           
        }
            catch
            {
                return false;
            }
}

        public static async Task<AuthOutput> PostToFacebookAsync(string message)
        {

            var IsAuth = await CheckFbAuthAsync();
            if (!IsAuth)
            {
                await AuthenticateToFacebookAsync();
            }
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string access_token = await GetFbTokenAsync();
     
                   var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("message", message)
                    });
                    var response = await httpClient.PostAsync(new Uri("https://graph.facebook.com/v2.6/me/feed?access_token=" +
                        access_token)
                            , content);
                    if(response.IsSuccessStatusCode)
                    {
                        return (new AuthOutput
                        {
                            Output = response.ToString(),
                            InfoTag = "Success"
                        });
                    }
                    else
                    {
                        return (new AuthOutput
                        {
                          InfoTag = "Unsuccessful"
                        });
                    }

                 }
              }
            catch (Exception e)
            {
                return (new AuthOutput
                {
                    Output = e.Message,
                    InfoTag = "Exception"
                });
            }
        }

        public static async Task<AuthOutput> Tweet(string message)
        {
            var IsAuth = await CheckTwitterAuthAsync();
            if(! IsAuth)
            {
                return (new AuthOutput
                {
                    InfoTag = "Unauthorized"
                });
            }
            else
            {
                var authorizer = await GetTwitterCredentials();
                var twitterContext = new TwitterContext(authorizer);

                if (message.Length > 140)
                {
                    return (new AuthOutput
                    {
                        InfoTag = "characterLimit"
                    });
                }
                else
                {
                    Status tweet = await twitterContext.TweetAsync(message);

                    if (tweet != null)
                    {
                        return (new AuthOutput
                        {
                            //Output = WebAuthenticationResult.ResponseData.ToString(),
                            InfoTag = "Success"
                        });
                    }
                    else
                    {
                        return (new AuthOutput
                        {
                            // Output = e.Message,
                            InfoTag = "Exception"
                        });
                    }
                }
      
            }
        }

        public static async Task<bool> CheckTwitterAuthAsync()
        {
            var tokenSec = await GettwitterTokenSecretAsync();
            if (tokenSec == null || tokenSec == "Unauthorized")
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> CheckFbAuthAsync()
        {
            string access_token = await GetFbTokenAsync();
            if (string.IsNullOrWhiteSpace(access_token) || access_token == "Unauthorized")
            {
                return false;
            }
            return true;
        }
        public static async Task<UniversalAuthorizer> GetTwitterCredentials()
        {
            var token = await GettwitterTokenAsync();
            string key = "twitter_key";
            string sec = "twitter_secret";
            var tokenSec = await GettwitterTokenSecretAsync();
            var authorizer = new UniversalAuthorizer
            {

                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = key,
                    ConsumerSecret = sec,
                    OAuthToken = token,
                    OAuthTokenSecret = tokenSec

                }
            };
            return authorizer;
        }

        #region cryptography functions for oauth 1.0
        //private static string Sha1Encrypt(string baseString, string keyString)
        //{
        //    var crypt = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
        //    var buffer = CryptographicBuffer.ConvertStringToBinary(baseString, BinaryStringEncoding.Utf8);
        //    var keyBuffer = CryptographicBuffer.ConvertStringToBinary(keyString, BinaryStringEncoding.Utf8);
        //    var key = crypt.CreateKey(keyBuffer);


        //    var sigBuffer = CryptographicEngine.Sign(key, buffer);
        //    string signature = CryptographicBuffer.EncodeToHexString(sigBuffer);
        //    return signature;
        //}


        //public static byte[] HmacSha1Sign(byte[] keyBytes, string message)
        //{
        //    var messageBytes = Encoding.UTF8.GetBytes(message);
        //    MacAlgorithmProvider objMacProv = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
        //    CryptographicKey hmacKey = objMacProv.CreateKey(keyBytes.AsBuffer());
        //    IBuffer buffHMAC = CryptographicEngine.Sign(hmacKey, messageBytes.AsBuffer());
        //    return buffHMAC.ToArray();

        //}
        #endregion

       
    }

    public class AuthOutput
    {
        public string InfoTag { get; set; }
        public string Output { get; set; }
    }


}
  