﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using goFriend.Services;
using System.Globalization;
using System.Threading.Tasks;
using goFriend.DataModel;

namespace goFriend
{
    public partial class App : Application
    {
        public static string AzureBackendUrl = "http://gofriend.azurewebsites.net";
        public static bool UseMockDataStore = true;
        public static bool IsUserLoggedIn { get; set; }
        public static Friend User { get; set; }
        public static IFacebookManager FaceBookManager = DependencyService.Get<IFacebookManager>();
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public static IFriendStore FriendStore;

        public static Task TaskGetMyGroups;
        public static IEnumerable<ApiGetGroupsModel> MyGroups;
        public static IEnumerable<ApiGetGroupsModel> AllGroups;

        public App()
        {
            Logger.Info("GoFriend starting new instance...");
            res.Culture = new CultureInfo("vi-VN");
            //res.Culture = new CultureInfo("");
            //Thread.CurrentThread.CurrentCulture = res.Culture;
            //Thread.CurrentThread.CurrentUICulture = res.Culture;

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = (Exception)args.ExceptionObject;
                Logger.Error("UnhandledException exception");
                Logger.Error(ex.ToString());
            };

            InitializeComponent();

            DependencyService.Register<FriendStore>();
            FriendStore = DependencyService.Get<IFriendStore>();
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            IsUserLoggedIn = Settings.IsUserLoggedIn;
            if (IsUserLoggedIn)
            {
                User = Settings.LastUser;
            }
            var appShell = new AppShell();
            appShell.RefreshTabs();
            MainPage = appShell;

            Initialize();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void DisplayMsgInfo(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept);
            });
            //Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept);
        }

        public static void DisplayMsgError(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert(res.MsgTitleError, message, res.Accept);
            });
            //Current.MainPage.DisplayAlert(res.MsgTitleError, message, res.Accept);
        }

        public static Task<bool> DisplayMsgQuestion(string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await Current.MainPage.DisplayAlert(res.MsgTitleInfo, message, res.Accept, res.Cancel);
                tcs.SetResult(result);
            });
            return tcs.Task;
        }

        public static void Initialize()
        {
            TaskGetMyGroups = new Task(() =>
            {
                try
                {
                    Logger.Debug("TaskGetMyGroups.BEGIN");
                    MyGroups = FriendStore.GetMyGroups().Result;
                }
                catch (AggregateException e)
                {
                    e.Handle(x =>
                    {
                        if (x is GoException goe)
                        {
                            Logger.Error(goe.ToString());
                            switch (goe.Msg.Code)
                            {
                                case MessageCode.UserTokenError:
                                    DisplayMsgError(res.MsgErrWrongToken);
                                    break;
                                default:
                                    DisplayMsgError(goe.Msg.Msg);
                                    break;
                            }
                            return true;
                        }

                        Logger.Error(x.ToString());
                        return true;
                        //return false; // Let anything else stop the application.
                    });
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                finally
                {
                    Logger.Debug("TaskGetMyGroups.END");
                }
            });
            TaskGetMyGroups.Start();
        }
    }
}
