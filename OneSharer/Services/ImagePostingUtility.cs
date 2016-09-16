using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using LinqToTwitter;
using System.Net.Http.Headers;
using System.Net.Http;

namespace OneSharer.Services
{
    class ImagePostingUtility : AuthServcie
    {
        public static StorageFile file { get; set; }

        public static async Task<string> SendImage(bool fbCheck, bool twitterCheck, string caption)
        {
            var photoByteArray = await GetByteArray(file);

            #region facebook only
            if (fbCheck == true && twitterCheck == false)
            {
                return await FbPhotoHelper(caption);
            }
            #endregion

            #region twitter only
            else if (twitterCheck == true && fbCheck == false)
            {
                return await TwitterMediaHeper(photoByteArray, caption);
            }
            #endregion


            else if (fbCheck == true && twitterCheck == true)
            {
                bool fb = false, tw = false;     //useful bools

                var respFb = await FbPhotoHelper(caption);     //helpful helper

                if (respFb.Equals("Photo posted successfully"))
                {
                    fb = true;
                }
               var respTw = await TwitterMediaHeper(photoByteArray, caption);

                if (respTw == "Photo Tweeted sucessfully")
                {
                    tw = true;
                }
                else if(respTw == "Tweet failed")
                {
                    tw = false;
                }

                if (fb == true && tw == true) //Both done
                {
                    return "Photo posted and tweeted succesfully";
                }
                else if (fb == false && tw == true) //Tweet done
                {
                    return "Photo tweeted succesfully. Could not be posted to Facebook";
                }
                else if (fb == true && tw == false)  //Facebook done
                {
                    return "Photo posted succesfully. Could not be tweeted.";
                }
            }

            return "Post failed.";
        }

        public async static Task<byte[]> GetByteArray(StorageFile file)
        {
            using (var inputStream = await file.OpenSequentialReadAsync())
            {
                var readStream = inputStream.AsStreamForRead();

                var byteArray = new byte[readStream.Length];
                await readStream.ReadAsync(byteArray, 0, byteArray.Length);
                return byteArray;
            }
        }

        private async static Task<string> FbPhotoHelper(string caption)
        {

            string access_token = await GetFbTokenAsync();
            if (string.IsNullOrWhiteSpace(access_token) || access_token == "Unauthorized")
            {
                await AuthenticateToFacebookAsync();
            }
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] array = Encoding.ASCII.GetBytes(caption);
                string encodedCaption = utf8.GetString(array);

                string postURL = "https://graph.facebook.com/v2.6/me/photos?caption="+encodedCaption+"&access_token=" +
                   access_token;

                using (var httpClient = new HttpClient())
                {
                  
                    MultipartFormDataContent form = new MultipartFormDataContent();
                    HttpContent content = new StringContent("fileName");
                    form.Add(content, file.Name);                   
                    var stream = await file.OpenStreamForReadAsync();
                    content = new System.Net.Http.StreamContent(stream);
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "source",
                        FileName = file.Name,            
                     };

                    form.Add(content);

                    var response = await httpClient.PostAsync(postURL,form);
                    if (response.IsSuccessStatusCode)
                    {
                        return "Photo posted successfully";
                    }
                    else
                    {
                        return "Photo could not be posted";
                    }
                }
            }
            catch
            {
                return "Post failed.";
            }
        }

        private async static Task<string> TwitterMediaHeper(byte[] media,string caption)
        {
            bool isPng = false;
            if (ImageExtension(file.Name) == 1)
            {
               isPng = true;
            }
          
            var IsAuth = await CheckTwitterAuthAsync();
            if (!IsAuth)
            {
                var outPut = await AuthServcie.AuthenticateToTwitterAsync();
                if (outPut)
                {
                    return await TweetMedia(media, isPng, caption);
                }
                else
                {
                    return "You were NOT authorized succesfully";
                }
            }
            else
            {
                return await TweetMedia(media,isPng ,caption);
            }
      
        }

        private async static Task<string> TweetMedia(byte[] media,bool isPng,string caption)
        {
            try
            {
                string imageType;
                var authorizer = await GetTwitterCredentials();
                var twitterContext = new TwitterContext(authorizer);
                if(isPng)
                {
                    imageType = "image/png";
                }
                else
                {
                    imageType = "image/jpeg";
                }
                var outMedia = await twitterContext.UploadMediaAsync(media, imageType);
                var output = await twitterContext.TweetAsync(caption, outMedia.MediaID.ToEnumerable<ulong>());
                if (output != null)
                {
                    return "Photo Tweeted sucessfully";
                }
                else
                {
                    return "Tweet failed";
                }
            }
            catch
            {
                return "Tweet failed";
            }
        
        }

        private static int ImageExtension(string source)
        {
            if (source.EndsWith(".png"))
            {
                return 1;
            }
            else
            {
                return 2;
            }

        }

    }
}
