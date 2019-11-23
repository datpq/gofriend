using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web;
using Acr.UserDialogs;
using goFriend.DataModel;
using Plugin.Connectivity;
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

        private static void Validate()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new GoException(Message.MsgNoInternet);
            }
        }

        //private static bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<Friend> LoginWithFacebook(string authToken, string deviceInfo)
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                Logger.Debug($"LoginWithFacebook.BEGIN(authToken={authToken}, deviceInfo={deviceInfo})");
                UserDialogs.Instance.ShowLoading(res.Processing);

                Validate();

                if (authToken == null) return null;

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

            Validate();
            if (friend == null) return false;

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

        public async Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            GroupFixedCatValues result = null;
            try
            {
                Logger.Debug($"GetGroupFixedCatValues.BEGIN(groupId={groupId}, useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroupFixedCatValues/{App.User.Id}/{groupId}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<GroupFixedCatValues>(jsonString.Result);
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetGroupFixedCatValues.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<ApiGetGroupCatValuesModel>> GetGroupCatValues(int groupId, bool useCache = true, params string[] arrCatValues)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<ApiGetGroupCatValuesModel> result = null;
            try
            {
                Logger.Debug($"GetGroupCatValues.BEGIN(groupId={groupId}, useCache={useCache}, arrCatValues.Length={arrCatValues.Length}. {string.Join(", ", arrCatValues)})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroupCatValues/{App.User.Id}/{groupId}/{useCache}";
                if (arrCatValues.Length > 0)
                {
                    for (var i = 0; i < arrCatValues.Length; i++)
                    {
                        var sep = i == 0 ? "?" : "&";
                        requestUrl = $"{requestUrl}{sep}Cat{i}={HttpUtility.UrlEncode(arrCatValues[i])}";
                    }
                }
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<ApiGetGroupCatValuesModel>>(jsonString.Result);
                    //result = await response.Content.ReadAsAsync<IEnumerable<ApiGetGroupCatValuesModel>>();
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetGroupCatValues.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<GroupFriend>> GetGroupFriends(int groupId, bool isActive, bool useCache = true, params string[] arrCatValues)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<GroupFriend> result = null;
            try
            {
                Logger.Debug($"GetGroupFriends.BEGIN(groupId={groupId}, isActive={isActive}, useCache={useCache}, arrCatValues.Length={arrCatValues.Length}. {string.Join(", ", arrCatValues)})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroupFriends/{App.User.Id}/{groupId}/{isActive}/{useCache}";
                if (arrCatValues.Length > 0)
                {
                    for (var i = 0; i < arrCatValues.Length; i++)
                    {
                        var sep = i == 0 ? "?" : "&";
                        requestUrl = $"{requestUrl}{sep}Cat{i}={HttpUtility.UrlEncode(arrCatValues[i])}";
                    }
                }
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<GroupFriend>>(jsonString.Result);
                    //result = await response.Content.ReadAsAsync<IEnumerable<ApiGetGroupCatValuesModel>>();
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetGroupFriends.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<ApiGetGroupsModel>> GetMyGroups(bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<ApiGetGroupsModel> result = null;
            try
            {
                Logger.Debug($"GetMyGroups.BEGIN(useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetMyGroups/{App.User.Id}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<ApiGetGroupsModel>>(jsonString.Result);
                    //result = await response.Content.ReadAsAsync<IEnumerable<ApiGetGroupsModel>>();
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetMyGroups.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<ApiGetGroupsModel>> GetGroups(string searchText = null, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<ApiGetGroupsModel> result = null;
            try
            {
                Logger.Debug($"GetGroups.BEGIN(searchText={searchText}, useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroups/{App.User.Id}/{useCache}?searchText={searchText}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<ApiGetGroupsModel>>(jsonString.Result);
                    //result = await response.Content.ReadAsAsync<IEnumerable<ApiGetGroupsModel>>();
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message});
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetGroups.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<Friend> GetFriend(int groupId, int otherFriendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                Logger.Debug($"GetFriend.BEGIN(groupId={groupId}, otherFriendId={otherFriendId}, useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetFriend/{App.User.Id}/{groupId}/{otherFriendId}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<Friend> (jsonString.Result);
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetFriend.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<Notification>> GetNotifications(bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<Notification> result = null;
            try
            {
                Logger.Debug($"GetNotifications.BEGIN(useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetNotifications/{App.User.Id}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<Notification>>(jsonString.Result);
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    //var msg = await response.Content.ReadAsAsync<Message>();
                    throw new GoException(msg);
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetNotifications.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<bool> GroupSubscriptionReact(int groupFriendId, UserType userRight)
        {
            var stopWatch = Stopwatch.StartNew();
            var result = false;
            try
            {
                Logger.Debug($"GroupSubscriptionReact.BEGIN(groupFriendId={groupFriendId}, userRight={userRight})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GroupSubscriptionReact/{App.User.Id}/{groupFriendId}/{userRight}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.PostAsync(requestUrl, new StringContent(string.Empty, Encoding.UTF8, "application/json"));

                result = response.IsSuccessStatusCode;
                if (!result)
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GroupSubscriptionReact.END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<bool> SubscribeGroup(GroupFriend groupFriend)
        {
            var stopWatch = Stopwatch.StartNew();
            var result = false;
            try
            {
                Logger.Debug($"SubscribeGroup.BEGIN(groupFriend={groupFriend})");

                Validate();

                var serializedObject = JsonConvert.SerializeObject(groupFriend);

                var client = GetSecuredHttpClient();
                var requestUrl = "api/Friend/GroupSubscription";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.PostAsync(requestUrl, new StringContent(serializedObject, Encoding.UTF8, "application/json"));

                result = response.IsSuccessStatusCode;
                if (!result)
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (GoException e)
            {
                Logger.Error($"Error: {e.Msg}");
                throw;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw new GoException(new Message { Code = MessageCode.Unknown, Msg = e.Message });
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"SubscribeGroup.END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}