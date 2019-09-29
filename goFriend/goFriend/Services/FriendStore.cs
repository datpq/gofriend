using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Acr.UserDialogs;
using goFriend.DataModel;
using Xamarin.Forms;

namespace goFriend.Services
{
    public class FriendStore : IFriendStore
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private static HttpClient GetHttpClient()
        {
            return new HttpClient { BaseAddress = new Uri($"{App.AzureBackendUrl}/") };
        }

        private static HttpClient GetSecuredHttpClient()
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Add("token", App.User.Token.ToString());
            Logger.Debug($"id={App.User.Id}, token={App.User.Token}");
            return client;
        }

        private static bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<Friend> LoginWithFacebook(string authToken, string deviceInfo)
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                Logger.Debug($"LoginWithFacebook.BEGIN(authToken={authToken}, deviceInfo={deviceInfo})");
                UserDialogs.Instance.ShowLoading(res.Processing);

                if (authToken == null || !IsConnected)
                    return null;

                var client = GetHttpClient();
                client.DefaultRequestHeaders.Add("authToken", authToken);
                client.DefaultRequestHeaders.Add("deviceInfo", deviceInfo);

                var response = await client.GetAsync($"api/Friend/LoginWithFacebook");
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsAsync<Friend>();
                }
                else
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug($"LoginWithFacebook.END({result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<bool> SaveBasicInfo(Friend friend)
        {
            Logger.Debug($"SaveBasicInfo.BEGIN({friend})");
            if (friend == null || !IsConnected)
                return false;

            var serializedFriend = JsonConvert.SerializeObject(friend);

            UserDialogs.Instance.ShowLoading(res.Processing);
            var client = GetSecuredHttpClient();
            var response = await client.PutAsync($"api/Friend/SaveBasicInfo", new StringContent(serializedFriend, Encoding.UTF8, "application/json"));

            var result = false;
            if (response.IsSuccessStatusCode)
            {
                result = true;
            }
            else
            {
                var msg = await response.Content.ReadAsAsync<Message>();
                Logger.Error($"Error: {msg}");
            }
            UserDialogs.Instance.HideLoading();

            Logger.Debug($"SaveBasicInfo.END({result})");
            return result;
        }

        public async Task<IEnumerable<Tuple<Group, bool, bool>>> GetGroups()
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<Tuple<Group, bool, bool>> result = null;
            try
            {
                Logger.Debug("GetGroups.BEGIN");
                UserDialogs.Instance.ShowLoading(res.Processing);

                if (!IsConnected) return null;

                var client = GetSecuredHttpClient();
                var response = await client.GetAsync($"api/Friend/GetGroups/{App.User.Id}");
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<Tuple<Group, bool, bool>>>(jsonString.Result);
                    //result = await response.Content.ReadAsAsync<IEnumerable<Tuple<Group, bool, bool>>>();
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug($"GetGroups.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}