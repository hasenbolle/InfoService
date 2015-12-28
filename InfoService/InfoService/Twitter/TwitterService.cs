﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using FeedReader.Data;
using InfoService.Enums;
using InfoService.Utils.NotificationBar;
using InfoService.Utils;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using TwitterConnector;
using TwitterConnector.Data;
using TwitterConnector.Expections;
using TwitterConnector.OAuth;

namespace InfoService.Twitter
{
    public static class TwitterService
    {
        #region Config Properties

        private static decimal _refreshInterval;

        public static decimal RefreshInterval
        {
            get { return _refreshInterval; }
            set
            {
                logger.WriteLog("Set TwitterRefreshInterval to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _refreshInterval = value;
            }
        }

        private static bool _updateOnStartup;

        public static bool UpdateOnStartup
        {
            get { return _updateOnStartup; }
            set
            {
                logger.WriteLog("Set TwitterUpdateOnStartup to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _updateOnStartup = value;
            }
        }

        private static string _separator;

        public static string Separator
        {
            get { return _separator; }
            set
            {
                logger.WriteLog("Set TwitterSeparator to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _separator = value;
            }
        }

        private static string _postWatchMoviesMask;

        public static string PostWatchMoviesMask
        {
            get { return _postWatchMoviesMask; }
            set
            {
                logger.WriteLog("Set TwitterWatchMoviesMask to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _postWatchMoviesMask = value;
            }
        }

        private static string _postWatchSeriesMask;

        public static string PostWatchSeriesMask
        {
            get { return _postWatchSeriesMask; }
            set
            {
                logger.WriteLog("Set TwitterWatchSeriesMask to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _postWatchSeriesMask = value;
            }
        }

        private static bool _postWatchingVideos;

        public static bool PostWatchingVideos
        {
            get { return _postWatchingVideos; }
            set
            {
                logger.WriteLog("Set TwitterUpdateStatusOnVideoStart to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _postWatchingVideos = value;
            }
        }

        private static bool _postUsingMovingPictures;

        public static bool PostUsingMovingPictures
        {
            get { return _postUsingMovingPictures; }
            set
            {
                logger.WriteLog("Set TwitterUpdateUsingMovingPictures to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _postUsingMovingPictures = value;
            }
        }

        private static bool _postUsingTvSeries;

        public static bool PostUsingTVSeries
        {
            get { return _postUsingTvSeries; }
            set
            {
                logger.WriteLog("Set TwitterUpdateUsingMPTVSeries to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _postUsingTvSeries = value;
            }
        }

        private static bool _postUsingMyVideos;

        public static bool PostUsingMyVideos
        {
            get { return _postUsingMyVideos; }
            set
            {
                logger.WriteLog("Set TwitterUpdateUsingMyVideos to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _postUsingMyVideos = value;
            }
        }

        private static decimal _items;

        public static decimal Items
        {
            get { return _items; }
            set
            {
                logger.WriteLog("Set TwitterItemsCount to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _items = value;
            }
        }

        //private static bool _usePublicTimeline;
        //public static bool UsePublicTimeline
        //{
        //    get { return _usePublicTimeline; }
        //    set
        //    {
        //        logger.WriteLog("Set TwitterUsePublicTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
        //        _usePublicTimeline = value;
        //        if (value) UsedTimelines.Add(TimelineType.Public);
        //    }
        //}

        private static bool _useHomeTimeline;

        public static bool UseHomeTimeline
        {
            get { return _useHomeTimeline; }
            set
            {
                logger.WriteLog("Set TwitterUseHomeTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _useHomeTimeline = value;
                if (value) UsedTimelines.Add(TimelineType.Home);

            }
        }

        private static bool _useUserTimeline;

        public static bool UseUserTimeline
        {
            get { return _useUserTimeline; }
            set
            {
                logger.WriteLog("Set TwitterUseUserTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _useUserTimeline = value;
                if (value) UsedTimelines.Add(TimelineType.User);
            }
        }

        //private static bool _useFriendsTimeline;
        //public static bool UseFriendsTimeline
        //{
        //    get { return _useFriendsTimeline; }
        //    set
        //    {
        //        logger.WriteLog("Set TwitterUseFriendsTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
        //        _useFriendsTimeline = value;
        //        if (value) UsedTimelines.Add(TimelineType.Friends);
        //    }
        //}

        private static bool _useMentionsTimeline;

        public static bool UseMentionsTimeline
        {
            get { return _useMentionsTimeline; }
            set
            {
                logger.WriteLog("Set TwitterUseMentionsTimeline to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _useMentionsTimeline = value;
                if (value) UsedTimelines.Add(TimelineType.Mentions);
            }
        }

        //private static bool _useRetweetedByMeTimeline;
        //public static bool UseRetweetedByMeTimeline
        //{
        //    get { return _useRetweetedByMeTimeline; }
        //    set
        //    {
        //        logger.WriteLog("Set TwitterUseRetweetedByMeTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
        //        _useRetweetedByMeTimeline = value;
        //        if (value) UsedTimelines.Add(TimelineType.RetweetedByMe);
        //    }
        //}

        //private static bool _useRetweetedToMeTimeline;
        //public static bool UseRetweetedToMeTimeline
        //{
        //    get { return _useRetweetedToMeTimeline; }
        //    set
        //    {
        //        logger.WriteLog("Set TwitterUseRetweetedToMeTimeline to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
        //        _useRetweetedToMeTimeline = value;
        //        if (value) UsedTimelines.Add(TimelineType.RetweetedToMe);
        //    }
        //}

        private static bool _useRetweetsOfMeTimeline;

        public static bool UseRetweetsOfMeTimeline
        {
            get { return _useRetweetsOfMeTimeline; }
            set
            {
                logger.WriteLog("Set TwitterUseRetweetsOfMeTimeline to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _useRetweetsOfMeTimeline = value;
                if (value) UsedTimelines.Add(TimelineType.RetweetsOfMe);
            }
        }

        private static DeleteCache _deletionInterval;

        public static DeleteCache DeletionInterval
        {
            get { return _deletionInterval; }
            set
            {
                logger.WriteLog("Set TwitterDeletionInterval to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _deletionInterval = value;
            }
        }

        private static string _tickerMask;

        public static string TickerMask
        {
            get { return _tickerMask; }
            set
            {
                logger.WriteLog("Set TwitterTickerMask to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _tickerMask = value;
            }
        }

        private static string _cacheFolder;

        public static string CacheFolder
        {
            get { return _cacheFolder; }
            set
            {
                logger.WriteLog("Set CacheFolder to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _cacheFolder = value;
            }
        }

        private static bool _showPopup;

        public static bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                logger.WriteLog("Set TweetsShowPopup to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _showPopup = value;
            }
        }

        private static bool _popupWhileFullScreenVideo;

        public static bool PopupWhileFullScreenVideo
        {
            get { return _popupWhileFullScreenVideo; }
            set
            {
                logger.WriteLog("Set TweetsPopupWhileFullScreenVideo to " + value, LogLevel.Debug,
                    InfoServiceModul.InfoService);
                _popupWhileFullScreenVideo = value;
            }
        }

        private static decimal _popupTimeout;

        public static decimal PopupTimeout
        {
            get { return _popupTimeout; }
            set
            {
                logger.WriteLog("Set TweetsPopupTimeout to " + value, LogLevel.Debug, InfoServiceModul.InfoService);
                _popupTimeout = value;
            }
        }

        #endregion

        #region Fields

        private static TwitterConnector.Twitter _twitterTimelines;
        private static readonly Logger logger = Logger.GetInstance();

        #endregion

        #region Properties

        public static DateTime LastRefresh { get; set; }
        public static bool Enabled { get; set; }
        public static bool UpdateInProgress { get; set; }
        public static List<TimelineType> UsedTimelines = new List<TimelineType>();
        private static TimelineType _activeTimeline;

        public static TimelineType ActiveTimeline
        {
            get { return _activeTimeline; }
            set
            {
                _activeTimeline = value;
                logger.WriteLog("Set twitter timeline[" + value + "] as active timeline", LogLevel.Debug,
                    InfoServiceModul.Twitter);
            }
        }

        #endregion

        #region Methods

        public static bool SetupTwitter(string tokenValue, string tokenSecret)
        {
            try
            {
                LogEvents.OnDebug += new LogEvents.TwitterErrorHandler(LogEvents_OnDebug);
                LogEvents.OnError += new LogEvents.TwitterErrorHandler(LogEvents_OnError);
                LogEvents.OnInfo += new LogEvents.TwitterErrorHandler(LogEvents_OnInfo);
                LogEvents.OnWarning += new LogEvents.TwitterErrorHandler(LogEvents_OnWarning);
                TwitterConnector.Twitter.SetConsumerKeySecret(TwitterApiKeys.ConsumerKey, TwitterApiKeys.ConsumerSecret);
                _twitterTimelines = new TwitterConnector.Twitter(new AccessToken(tokenValue, tokenSecret), CacheFolder);
                Enabled = true;
                _activeTimeline = TimelineType.None;
                return true;
            }
            catch (TwitterNoPinExpection tnpe)
            {
                logger.WriteLog(tnpe.Message, LogLevel.Error, InfoServiceModul.Twitter);
                Enabled = false;
                return false;
            }
            catch (TwitterAuthExpection tae)
            {
                logger.WriteLog(tae.Message + ". " + tae.InnerException.Message, LogLevel.Error,
                    InfoServiceModul.Twitter);
                Enabled = false;
                return false;
            }
            catch (TwitterNoRequestTokenExpection tnrte)
            {
                logger.WriteLog(tnrte.Message, LogLevel.Error, InfoServiceModul.Twitter);
                Enabled = false;
                return false;
            }
            catch (TwitterUserPasswordExpection tupe)
            {
                logger.WriteLog(tupe.Message, LogLevel.Error, InfoServiceModul.Twitter);
                Enabled = false;
                return false;
            }
            catch (TwitterNoCacheFolderExpection tncfe)
            {
                logger.WriteLog(tncfe.Message, LogLevel.Error, InfoServiceModul.Twitter);
                _twitterTimelines = null;
                Enabled = false;
                return false;
            }
            catch (TwitterCacheFolderNotValid tcfnv)
            {
                logger.WriteLog(tcfnv.Message, LogLevel.Error, InfoServiceModul.Twitter);
                _twitterTimelines = null;
                Enabled = false;
                return false;
            }

        }



        public static Timeline GetTimeline(TimelineType type)
        {
            return type != TimelineType.None ? _twitterTimelines.Timelines[type.ToString()] : GetFirstUsedTimeline();
        }

        private static Timeline GetFirstUsedTimeline()
        {
            //if (UsePublicTimeline) return _twitterTimelines.Timelines[TimelineType.Public.ToString()];
            //if (UseFriendsTimeline) return _twitterTimelines.Timelines[TimelineType.Friends.ToString()];
            if (UseHomeTimeline) return _twitterTimelines.Timelines[TimelineType.Home.ToString()];
            if (UseUserTimeline) return _twitterTimelines.Timelines[TimelineType.User.ToString()];
            if (UseMentionsTimeline) return _twitterTimelines.Timelines[TimelineType.Mentions.ToString()];
            //if (UseRetweetedByMeTimeline) return _twitterTimelines.Timelines[TimelineType.RetweetedByMe.ToString()];
            //if (UseRetweetedToMeTimeline) return _twitterTimelines.Timelines[TimelineType.RetweetedToMe.ToString()];
            if (UseRetweetsOfMeTimeline) return _twitterTimelines.Timelines[TimelineType.RetweetsOfMe.ToString()];
            return null;
        }

        public static bool PostStatus(string message)
        {
            return _twitterTimelines.PostStatus(message);
        }

        public static void DeleteCache()
        {
            _twitterTimelines.DeleteCache();
        }

        public static bool UpdateTwitter()
        {
            if (!Enabled)
            {
                logger.WriteLog("Twitter is not enabled", LogLevel.Error, InfoServiceModul.Twitter);
                return false;
            }

            UpdateInProgress = true;
            try
            {
                if (ShowPopup)
                {
                    foreach (KeyValuePair<string, Timeline> timeline in _twitterTimelines.Timelines.Where(timeline => timeline.Value.Type != TimelineType.User))
                    {
                        timeline.Value.OnNewItems += Value_OnNewItems;
                    }
                }
                if (_twitterTimelines.UpdateTimelines(UsedTimelines, false))
                {
                    LastRefresh = DateTime.Now;
                    logger.WriteLog("Update of Twitter succesfull. Now setting up properties...", LogLevel.Info,
                        InfoServiceModul.Twitter);
                    return true;
                }
                else
                {
                    logger.WriteLog("Update of Twitter unsuccesfull. Check the errors and warnings above",
                        LogLevel.Error, InfoServiceModul.Twitter);
                    return false;
                }
            }
            finally
            {
                UpdateInProgress = false;
            }
        }

        private static void Value_OnNewItems(Timeline timeline, List<TwitterConnector.Data.TwitterItem> newItems)
        {

            logger.WriteLog("Popups are enabled and new tweetsloaded...", LogLevel.Info, InfoServiceModul.Twitter);

            bool notificationBarPluginEnabled = false;
            using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.MPSettings())
            {
                notificationBarPluginEnabled = settings.GetValueAsString("plugins", "MPNotificationBar", "no") == "yes";
            }
            if (InfoServiceUtils.IsAssemblyAvailable("MPNotificationBar", new Version(0, 8, 3, 0)) &&
                System.IO.File.Exists(GUIGraphicsContext.Skin + @"\NotificationBar.xml") &&
                notificationBarPluginEnabled)
            {
                string text = string.Empty;
                foreach (TwitterItem item in newItems)
                {
                    text += item.Text + "\n";
                }

                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Length >= 2) text = text.Substring(0, text.Length - 1);
                    if (PopupWhileFullScreenVideo || !GUIGraphicsContext.IsFullScreenVideo)
                    {
                        logger.WriteLog(
                            "Showing new Popup (NotificationBar) for Timeline[" + timeline.Type + "] with text \"" +
                            text + "\"", LogLevel.Info, InfoServiceModul.Twitter);
                        NotificationBar.ShowNotificationBar(
                            String.Format(InfoServiceUtils.GetLocalizedLabel(40), newItems.Count.ToString(), timeline.Type),
                            text, newItems[0].User.PicturePath, PopupWhileFullScreenVideo, (int) PopupTimeout);
                    }
                    else
                        logger.WriteLog(
                            "Showing new Popup (NotificationBar) for Timeline[" + timeline.Type + "] with text \"" +
                            text +
                            "\" is not allowed - Fullscreen Video is running...", LogLevel.Info,
                            InfoServiceModul.Twitter);
                }
            }
            else
            {
                string text = newItems.Count == 1
                    ? newItems[0].Text + " " + GUILocalizeStrings.Get(1024) + " @" + newItems[0].User.ScreenName
                    : String.Format(InfoServiceUtils.GetLocalizedLabel(40), timeline.Type);
                if (!string.IsNullOrEmpty(text))
                {
                    if (PopupWhileFullScreenVideo || !GUIGraphicsContext.IsFullScreenVideo)
                    {
                        logger.WriteLog(
                            "Showing new Popup (MediaPortal Dialog) for Timeline[" + timeline.Type + "] with text \"" +
                            text +
                            "\"", LogLevel.Info, InfoServiceModul.Feed);
                        InfoServiceUtils.ShowDialogNotifyWindow(
                            String.Format(InfoServiceUtils.GetLocalizedLabel(40), newItems.Count.ToString(),
                                timeline.Type),
                            text, newItems[0].User.PicturePath, new Size(120, 120), (int) PopupTimeout);
                    }
                    else
                        logger.WriteLog(
                            "Showing new Popup (MediaPortal Dialog) for Timeline[" + timeline.Type + "] with text \"" +
                            text +
                            "\" is not allowed - Fullscreen Video is running...", LogLevel.Info,
                            InfoServiceModul.Twitter);
                }
            }
        }



        #endregion

        #region Log Events

        static void LogEvents_OnWarning(TwitterArgs twitterArguments)
        {
            logger.WriteLog(twitterArguments, LogLevel.Warning, InfoServiceModul.Twitter);
        }

        static void LogEvents_OnInfo(TwitterArgs twitterArguments)
        {
            logger.WriteLog(twitterArguments, LogLevel.Info, InfoServiceModul.Twitter);
        }

        static void LogEvents_OnError(TwitterArgs twitterArguments)
        {
            logger.WriteLog(twitterArguments, LogLevel.Error, InfoServiceModul.Twitter);
        }

        static void LogEvents_OnDebug(TwitterArgs twitterArguments)
        {
            logger.WriteLog(twitterArguments, LogLevel.Debug, InfoServiceModul.Twitter);
        }

        #endregion
    }
}
