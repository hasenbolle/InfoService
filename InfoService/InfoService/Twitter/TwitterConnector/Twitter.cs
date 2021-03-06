﻿#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TwitterConnector.Expections;
using TwitterConnector.OAuth;

#endregion

namespace TwitterConnector
{
    public class Twitter
    {
        internal static string ConsumerKey = string.Empty;
        internal static string ConsumerSecret = string.Empty;
        private AccessToken _accessToken;
        private static RequestToken _reqToken;

        public static string CacheFolder { get; protected set; }
        public Dictionary<string, Timeline> Timelines { get; set; }
        public static bool CacheEnabled { get; private set; }
        public static bool CacheAutomatic { get; private set; }

        public static string GetAuthUrl()
        {
            if (_reqToken != null)
            {
                try
                {
                    return Consumer.BuildUserAuthorizationURL("https://api.twitter.com/oauth/authorize", _reqToken);
                }
                catch (Exception ex)
                {
                    throw new TwitterAuthExpection("Couldn't get auth url", ex);
                }
                
            }
            throw new TwitterAuthExpection("Couldn't get auth url. RequestToken is empty");
        }

        public static void SetConsumerKeySecret(string consumerKey, string consumerSecret)
        {
            ConsumerSecret = consumerSecret;
            ConsumerKey = consumerKey;
        }
            
        public static bool GetRequestToken(out RequestToken requestToken)
        {
            if (!string.IsNullOrEmpty(ConsumerKey) && !string.IsNullOrEmpty(ConsumerSecret))
            {
                try
                {
                    Consumer c = new Consumer(ConsumerKey, ConsumerSecret);
                    _reqToken = c.ObtainUnauthorizedRequestToken("https://api.twitter.com/oauth/request_token",
                        "https://twitter.com/");
                    requestToken = _reqToken;
                    return true;
                }
                catch (Exception ex)
                {
                    throw new TwitterAuthExpection("Couldn't get request token", ex);
                }
            }
            else
            {
                throw new TwitterAuthExpection("Couldn't get request token. Consumer key/secret is initial.");
            }

        }
        public static bool GetAuthToken(string pin, out AccessToken accessToken)
        {
            if (!string.IsNullOrEmpty(ConsumerKey) && !string.IsNullOrEmpty(ConsumerSecret))
            {
                try
                {
                    Consumer c = new Consumer(ConsumerKey, ConsumerSecret);
                    //_reqToken = c.ObtainUnauthorizedRequestToken("http://twitter.com/oauth/request_token", "https://twitter.com/");
                    accessToken = c.RequestAccessToken(pin, _reqToken, "https://api.twitter.com/oauth/access_token",
                        "https://twitter.com/");
                    return true;
                }
                catch (Exception ex)
                {
                    throw new TwitterAuthExpection("Couldn't get auth token", ex);
                }
            }
            else
            {
                throw new TwitterAuthExpection("Couldn't get request token. Consumer key/secret is initial.");
            }

        }

        public Twitter(AccessToken accessToken)
        {
            InitTwitter(accessToken);
        }
        public Twitter(AccessToken accessToken, string cacheFolder, bool useCacheAutomatic = false)
        {
            InitTwitter(accessToken);
            SetCache(cacheFolder);
            CacheAutomatic = useCacheAutomatic;
            if (!useCacheAutomatic) EnableCache();
        }

        private void InitTwitter(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new TwitterNoRequestTokenExpection("No token for OAuth is set. TwitterConnector disabled");
            }
            if (string.IsNullOrEmpty(accessToken.TokenValue))
            {
                throw new TwitterNoRequestTokenExpection("No token value for OAuth is set. TwitterConnector disabled");
            }
            if (string.IsNullOrEmpty(accessToken.TokenSecret))
            {
                throw new TwitterNoRequestTokenExpection("No token secret for OAuth is set. TwitterConnector disabled");
            }
            try
            {
                _accessToken = accessToken;
                LogEvents.InvokeOnDebug(new TwitterArgs("Create TwitterConnector(using OAuth) instance using cache"));
            }
            catch (Exception ex)
            {
                throw new TwitterAuthExpection("Authorization unsuccessful", ex);
            }

            Timelines = new Dictionary<string, Timeline>();
            //Timelines.Add(TimelineType.Public.ToString(), new Timeline(TimelineType.Public, authType, user, password, accessToken));
            //Timelines.Add(TimelineType.Friends.ToString(), new Timeline(TimelineType.Friends, authType, user, password, accessToken));
            Timelines.Add(TimelineType.User.ToString(), new Timeline(TimelineType.User, accessToken));
            Timelines.Add(TimelineType.Home.ToString(), new Timeline(TimelineType.Home, accessToken));
            Timelines.Add(TimelineType.Mentions.ToString(), new Timeline(TimelineType.Mentions, accessToken));
            //Timelines.Add(TimelineType.RetweetedByMe.ToString(), new Timeline(TimelineType.RetweetedByMe, authType, user, password, accessToken));
            //Timelines.Add(TimelineType.RetweetedToMe.ToString(), new Timeline(TimelineType.RetweetedToMe, authType, user, password, accessToken));
            Timelines.Add(TimelineType.RetweetsOfMe.ToString(), new Timeline(TimelineType.RetweetsOfMe, accessToken));
        }

        public static void DisableCache()
        {
            LogEvents.InvokeOnDebug(new TwitterArgs("Cache disabled."));
            CacheEnabled = false;

        }
        public static bool EnableCache()
        {
            LogEvents.InvokeOnDebug(new TwitterArgs("Try enabling cache..."));
            if (CacheEnabled)
            {
                LogEvents.InvokeOnDebug(new TwitterArgs("Cache already enabled with cache folder \"" + CacheFolder + "\""));
                return true;
            }
            return CheckCache();
        }
        public static bool CheckCache()
        {
            LogEvents.InvokeOnDebug(new TwitterArgs("Checking cache folder \"" + CacheFolder + "\"..."));
            if (!string.IsNullOrEmpty(CacheFolder))
            {
                if (Utils.IsValidPath(CacheFolder))
                {
                    LogEvents.InvokeOnDebug(new TwitterArgs(CacheFolder + " is a valid path. Now checking if the cache folder already exists..."));
                    if (Utils.IsCacheFolderAvailable(CacheFolder))
                    {

                        if (!Utils.DoesCacheFolderExists(CacheFolder))
                        {
                            LogEvents.InvokeOnDebug(new TwitterArgs(CacheFolder + " doesn't exist. Now create a new folder."));
                            try
                            {
                                Directory.CreateDirectory(CacheFolder);
                            }
                            catch (Exception ex)
                            {
                                LogEvents.InvokeOnWarning(new TwitterArgs("Could not create cache older " + CacheFolder, ex.Message));
                                DisableCache();
                                return false;
                                //throw new FeedCacheFolderNotValid("Could not create cache older " + cacheFolder + ". Feed disabled." + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        CacheEnabled = false;
                        LogEvents.InvokeOnWarning(new TwitterArgs("Cache folder " + CacheFolder + " is not available."));
                        DisableCache();
                        return false;
                        //throw new FeedCacheFolderNotValid(cacheFolder + " is not available in the network. Feed disabled.");
                    }
                    LogEvents.InvokeOnDebug(new TwitterArgs(CacheFolder + " exist. Caching for feeds is now enabled."));

                    //CacheFolder = cacheFolder;
                    CacheEnabled = true;
                    return true;

                }
                else
                {
                    LogEvents.InvokeOnWarning(new TwitterArgs("Cache folder " + CacheFolder + " is not valid path."));
                    DisableCache();
                    return false;
                    //throw new FeedCacheFolderNotValid(cacheFolder + " is not a valid path. Caching disabled");
                }
            }
            else
            {
                LogEvents.InvokeOnWarning(new TwitterArgs("Cache folder path is empty."));
                DisableCache();
                return false;
                //throw new FeedNoCacheFolderExpection("Cache folder path is empty. Caching disabled");
            }
        }
        public static void SetCache(string cacheFolder, bool autoEnableCache)
        {
            LogEvents.InvokeOnDebug(new TwitterArgs("Set cache folder to \"" + cacheFolder + "\""));
            DisableCache();
            CacheFolder = cacheFolder;
            if (!CacheFolder.EndsWith(@"\")) CacheFolder += @"\";
            if (autoEnableCache) EnableCache();
        }
        public static void SetCache(string cacheFolder)
        {
            SetCache(cacheFolder, false);
        }
       
        public bool DeleteCache()
        {
            LogEvents.InvokeOnDebug(new TwitterArgs("Try to delete cache from " + CacheFolder));
            if (Directory.Exists(CacheFolder))
            {
                try
                {
                    Directory.Delete(CacheFolder, true);
                    LogEvents.InvokeOnInfo(new TwitterArgs("Deleted cache successful"));
                    return true;
                }
                catch (Exception ex)
                {
                    LogEvents.InvokeOnError(new TwitterArgs("Error deleting cache", ex.Message, ex.StackTrace));
                }

            }
            LogEvents.InvokeOnInfo(new TwitterArgs("Error deleting cache. Directory " + CacheFolder + " doesn't exist"));
            return false;
        }

        public bool UpdateTimelines(List<TimelineType> timeLines, bool withRetweets)
        {
            int successCount = timeLines.Count;
            int count = 0;

            bool cacheAvailable = false;
            if (CacheAutomatic)
            {
                CheckCache();
            }

            if (CacheEnabled)
            {
                if (!CacheAutomatic)
                {
                    cacheAvailable = Utils.IsCacheFolderAvailable(CacheFolder);
                }
                else cacheAvailable = true;
            }

            foreach (KeyValuePair<string, Timeline> timeline in Timelines.Where(timeline => timeLines.Contains(timeline.Value.Type)))
            {
                if (!CacheEnabled)
                {
                    LogEvents.InvokeOnInfo(new TwitterArgs("Parsing twitter timeline " + timeline.Value.Type + " without using cache"));
                    if (timeline.Value.Update(withRetweets))
                    {
                        count++;
                    }
                }
                else
                {
                    if (cacheAvailable)
                    {
                        LogEvents.InvokeOnInfo(new TwitterArgs("Parsing twitter timeline " + timeline.Value.Type + " into cache folder " + CacheFolder));
                        if (timeline.Value.Update(withRetweets, CacheFolder))
                        {
                            count++;
                        }
                    }
                    else
                    {
                        if (CacheAutomatic)
                        {
                            LogEvents.InvokeOnInfo(new TwitterArgs("Parsing twitter timeline " + timeline.Value.Type + " without using cache"));
                            if (timeline.Value.Update(withRetweets))
                            {
                                count++;
                            }
                        }
                        else
                        {
                            LogEvents.InvokeOnError(new TwitterArgs("Error parsing timeline into cache folder " + CacheFolder + ". Cache folder is not available...", "")); 
                        }

                    }
                }
            }
            return successCount == count;
        }
        public bool UpdateAllTimelines(bool withRetweets)
        {
            return UpdateTimelines(new List<TimelineType>() { TimelineType.Home, TimelineType.Mentions, TimelineType.RetweetsOfMe, TimelineType.User }, withRetweets);
        }
        public bool PostStatus(string message)
        {
            try
            {
                Consumer c = new Consumer(ConsumerKey, ConsumerSecret);
                HttpWebResponse resp =
                    c.AccessProtectedResource(
                        _accessToken,
                        "https://api.twitter.com/1.1/statuses/update.json",
                        "POST",
                        "https://twitter.com/", new[] { new Parameter("status", message) });
                if (resp != null)
                {
                    resp.Close();
                }
                LogEvents.InvokeOnInfo(new TwitterArgs("Posting status update \"" + message + "\" successful"));
                
            }
            catch (Exception ex)
            {
                LogEvents.InvokeOnError(new TwitterArgs("Error posting status update", ex.Message, ex.StackTrace));
                return false;
            }
            return true;
        }
    }
}
