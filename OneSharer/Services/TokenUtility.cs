using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneSharer.Services
{
    class TokenUtility
    {
        protected static async Task<bool> SaveFbTokenAsync(string webAuthResultResponseData)
        {
            try
            {
                string responseData = webAuthResultResponseData.Substring(webAuthResultResponseData.IndexOf("access_token"));
                String[] keyValPairs = responseData.Split('&');
                string access_token = null;
                string expires_in = null;
                for (int i = 0; i < keyValPairs.Length; i++)
                {
                    String[] splits = keyValPairs[i].Split('=');
                    switch (splits[0])
                    {
                        case "access_token":
                            access_token = splits[1];
                            break;
                        case "expires_in":
                            expires_in = splits[1];
                            break;
                    }
                }

                StorageFile savedFile = await ApplicationData.Current.LocalFolder.
                    CreateFileAsync("fbToken.dat", CreationCollisionOption.ReplaceExisting);

                using (Stream writeStream = await savedFile.OpenStreamForWriteAsync())
                {
                    DataContractSerializer serializer =
                        new DataContractSerializer(typeof(string));

                    serializer.WriteObject(writeStream, access_token);
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected static async Task<bool> SaveTwitterTokenAsync(string token, string tokenSecret, string key, string secret)
        {
            try
            {
                StorageFile tokenFile = await ApplicationData.Current.LocalFolder.
                 CreateFileAsync("TwitterToken.dat", CreationCollisionOption.ReplaceExisting);
                StorageFile tokenSecretFile = await ApplicationData.Current.LocalFolder.
                CreateFileAsync("TwitterTokenSecret.dat", CreationCollisionOption.ReplaceExisting);


                using (Stream writeStream = await tokenFile.OpenStreamForWriteAsync())
                {
                    DataContractSerializer serializer =
                        new DataContractSerializer(typeof(string));

                    serializer.WriteObject(writeStream, token);
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }
                using (Stream writeStream = await tokenSecretFile.OpenStreamForWriteAsync())
                {
                    DataContractSerializer serializer =
                        new DataContractSerializer(typeof(string));

                    serializer.WriteObject(writeStream, tokenSecret);
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetFbTokenAsync()
        {
            try
            {
                var readStream =
                await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("fbToken.dat");
                if (readStream == null)
                {
                    return null;
                }
                DataContractSerializer serializer =
                    new DataContractSerializer(typeof(string));

                var token = serializer.ReadObject(readStream).ToString();
                return token;
            }
            catch (Exception)
            {
                return "Unauthorized";
            }
        }

        public static async Task<string> GettwitterTokenAsync()
        {
            try
            {
                var readStream =
                await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("TwitterToken.dat");
                if (readStream == null)
                {
                    return null;
                }
                DataContractSerializer serializer =
                    new DataContractSerializer(typeof(string));

                var token = serializer.ReadObject(readStream).ToString();
                return token;
            }
            catch (Exception)
            {
                return "Unauthorized";
            }
        }

        public static async Task<string> GettwitterTokenSecretAsync()
        {
            try
            {
                var readStream =
              await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("TwitterTokenSecret.dat");
                if (readStream == null)
                {
                    return null;
                }
                DataContractSerializer serializer =
                    new DataContractSerializer(typeof(string));

                var tokenSec = serializer.ReadObject(readStream).ToString();
                return tokenSec;
            }
            catch (Exception)
            {
                return "Unauthorized";
            }

        }

        protected static async Task<string> CallForLonglivedTokenAsync(string webAuthResultResponseData)
        {

            string responseData = webAuthResultResponseData.Substring(webAuthResultResponseData.IndexOf("access_token"));
            String[] keyValPairs = responseData.Split('&');
            string shortLivedToken = null;
            string expires_in = null;
            for (int i = 0; i < keyValPairs.Length; i++)
            {
                String[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "access_token":
                        shortLivedToken = splits[1];
                        break;
                    case "expires_in":
                        expires_in = splits[1];
                        break;
                }
            }
            using (var httpClient = new HttpClient())
            {
                var response =
                 await httpClient.GetStringAsync(new Uri("https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=" +
                         Uri.EscapeDataString("facebook_client_id") +
                         "&client_secret=" +
                         Uri.EscapeDataString("facebook_client_secret") +
                         "&fb_exchange_token=" +
                         Uri.EscapeDataString(shortLivedToken)));

                return response;
            }
        }


    }
}
