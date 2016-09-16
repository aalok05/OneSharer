using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneSharer.Services
{
    class PostingUtility
    {
        public static async Task<string> Send(bool fbCheck, bool twitterCheck, string message)
        {
            AuthOutput respTw;

            #region facebook only

            if (fbCheck == true && twitterCheck == false)
            {
              return await FbHelperAsync(message);
            }
            #endregion


            #region twitter only
            else if (twitterCheck == true && fbCheck == false)
            {
                respTw = await AuthServcie.Tweet(message);
                if (respTw.InfoTag == "Success")
                {

                    return "Message tweeeted succesfully";
                }
                if (respTw.InfoTag == "Unauthorized")
                {
                    var outPut = await AuthServcie.AuthenticateToTwitterAsync();
                    if (outPut)
                    {
                        respTw = await AuthServcie.Tweet(message);
                        if (respTw.InfoTag == "Success")
                        {
                            return "Message tweeeted succesfully";
                        }
                    }
                    else
                    {
                        return "You were NOT authorized succesfully";
                    }
                }
            }
            #endregion

            else if (fbCheck == true && twitterCheck == true)
            {
                bool fb = false, tw = false;     //useful bools

                var respFb = await FbHelperAsync(message);     //helpful helper

                if(respFb.Equals("Message posted succesfully"))
                {
                    fb = true;
                }
                respTw = await AuthServcie.Tweet(message);

                if (respTw.InfoTag == "Success")
                {
                    tw = true;                   
                }
               else if (respTw.InfoTag == "Unauthorized")
                {
                    var outPut = await AuthServcie.AuthenticateToTwitterAsync();
                    if (outPut)
                    {
                        respTw = await AuthServcie.Tweet(message);
                        if (respTw.InfoTag == "Success")
                        {
                            tw = true;
                        }
                    }
                    else
                    {                    
                      tw = false;
                    }
                }
              
                if(fb == true && tw == true) //Both done
                {
                    return "Message posted and tweeted succesfully";
                }
                else if(fb == false && tw == true) //Tweet done
                {
                    return "Message tweeted succesfully. Could not be posted to Facebook";
                }
                else if (fb == true && tw == false)  //Facebook done
                {
                    return "Message posted succesfully. Could not be tweeted.";
                }
            }
            return "Message could not be posted.";    // have to return something
        }

        private static async Task<string> FbHelperAsync(string message)
        {

            AuthOutput respFb;

            respFb = await AuthServcie.PostToFacebookAsync(message);
            if (respFb.InfoTag == "Success")
            {
                return "Message posted succesfully";
            }
            else if (respFb.InfoTag == "Unsuccessful")
            {
                return "Unexpected problem. Try authenticating again";
            }
            else if (respFb.InfoTag == "Exception")
            {
                var outPut = await AuthServcie.AuthenticateToFacebookAsync();
                if (outPut.InfoTag.Equals("Succses"))
                {
                    respFb = await AuthServcie.PostToFacebookAsync(message);
                    if (respFb.InfoTag.Equals("Succses"))
                    {
                        return "Message posted succesfully";
                    }
                    else if (respFb.InfoTag == "Unsuccessful")
                    {
                        return "Unexpected problem. Try authenticating again";
                    }
                }
                else if (outPut.InfoTag.Equals("UserCancel"))
                {
                    return "You were NOT authorized to Facebook succesfully";
                }
                else
                { 
                    return "Unexpected problem. Try authenticating again";
                }

            }
           
            return "Unexpected problem. Try Authenticating again.";
        }    

    }
   }
        
 

