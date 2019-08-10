using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System;
using goFriend.DataModel;

namespace goFriend.Services
{
    public class FriendStore : IFriendStore
    {
        HttpClient client;
        public FriendStore()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri($"{App.AzureBackendUrl}/");
        }

        bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<bool> AddOrUpdateFriendAsync(Friend friend)
        {
            if (friend == null || !IsConnected)
                return false;

            var serializedItem = JsonConvert.SerializeObject(friend);

            var response = await client.PostAsync($"api/Friend", new StringContent(serializedItem, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }
    }
}