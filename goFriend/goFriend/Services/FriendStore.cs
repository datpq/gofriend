using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System;
using Acr.UserDialogs;
using goFriend.DataModel;
using Xamarin.Forms;

namespace goFriend.Services
{
    public class FriendStore : IFriendStore
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private readonly HttpClient _client;
        public FriendStore()
        {
            _client = new HttpClient {BaseAddress = new Uri($"{App.AzureBackendUrl}/")};
        }

        private static bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<Friend> LoginWithFacebook(Friend friend)
        {
            Logger.Debug($"LoginWithFacebook.BEGIN({friend})");
            if (friend == null || !IsConnected)
                return null;

            var serializedFriend = JsonConvert.SerializeObject(friend);

            UserDialogs.Instance.ShowLoading(res.Processing);
            var response = await _client.PostAsync($"api/Friend/LoginWithFacebook", new StringContent(serializedFriend, Encoding.UTF8, "application/json"));

            Friend result = null;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<Friend>();
            }
            else
            {
                var msg = await response.Content.ReadAsAsync<Message>();
                Logger.Error($"Error: {msg}");
            }
            UserDialogs.Instance.HideLoading();

            Logger.Debug($"LoginWithFacebook.END({result})");
            return result;
        }

        public async Task<bool> SaveBasicInfo(Friend friend)
        {
            Logger.Debug($"SaveBasicInfo.BEGIN({friend})");
            if (friend == null || !IsConnected)
                return false;

            var serializedFriend = JsonConvert.SerializeObject(friend);

            UserDialogs.Instance.ShowLoading(res.Processing);
            var response = await _client.PutAsync($"api/Friend/SaveBasicInfo", new StringContent(serializedFriend, Encoding.UTF8, "application/json"));

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
    }
}