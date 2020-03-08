using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using goFriend.DataModel;
using Microsoft.Extensions.Caching.Memory;
using PCLAppConfig;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace goFriend.Services
{
    public class FriendStore : IFriendStore
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private const string CacheTimeoutPrefix = "CacheTimeout.";
        private readonly IMemoryCache _memoryCache;

        public FriendStore()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        private static HttpClient GetHttpClient()
        {
            return new HttpClient { BaseAddress = new Uri($"{ConfigurationManager.AppSettings["AzureBackendUrl1010"]}/") };
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

        #region Cache Functions

        static string GetActualAsyncMethodName([CallerMemberName]string name = null) => name;

        #endregion
        //private static bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<Friend> LoginWithThirdParty(Friend friend, string deviceInfo) //string thirdPartyToken, 
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                Logger.Debug($"LoginWithThirdParty.BEGIN(friend={friend}, deviceInfo={deviceInfo})");//thirdPartyToken={thirdPartyToken}, 

                Validate();

                //if (thirdPartyToken == null) return null;

                var client = GetHttpClient();
                //client.DefaultRequestHeaders.Add("thirdPartyToken", thirdPartyToken);
                client.DefaultRequestHeaders.Add("deviceInfo", deviceInfo);

                var serializedObject = JsonConvert.SerializeObject(friend);
                var response = await client.PutAsync($"api/Friend/LoginWithThirdParty", new StringContent(serializedObject, Encoding.UTF8, "application/json"));
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
                Logger.Debug($"LoginWithThirdParty.END({result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<Friend> LoginWithFacebook(string authToken, string deviceInfo)
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                var info = Extension.GetVersionTrackingInfo();
                Logger.Debug($"LoginWithFacebook.BEGIN(authToken={authToken}, deviceInfo={deviceInfo}, info={info})");

                Validate();

                if (authToken == null) return null;

                var client = GetHttpClient();
                client.DefaultRequestHeaders.Add("authToken", authToken);
                client.DefaultRequestHeaders.Add("deviceInfo", deviceInfo);
                client.DefaultRequestHeaders.Add("info", info);

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
                Logger.Debug($"LoginWithFacebook.END({result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<bool> SaveBasicInfo(Friend friend)
        {
            Logger.Debug($"SaveBasicInfo.BEGIN({friend})");

            Validate();
            if (friend == null) return false;

            var serializedFriend = JsonConvert.SerializeObject(friend);

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

            Logger.Debug($"SaveBasicInfo.END({result})");
            return result;
        }

        public async Task<GroupFixedCatValues> GetGroupFixedCatValues(int groupId, bool useClientCache = true, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            GroupFixedCatValues result = null;
            try
            {
                Logger.Debug($"GetGroupFixedCatValues.BEGIN(groupId={groupId}, useClientCache={useClientCache}, useCache={useCache})");

                var cachePrefix = $"{CacheTimeoutPrefix}{GetActualAsyncMethodName()}";
                var cacheTimeout = int.Parse(ConfigurationManager.AppSettings[cachePrefix]);
                var cacheKey = $"{cachePrefix}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useClientCache)
                {
                    result = _memoryCache.Get(cacheKey) as GroupFixedCatValues;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

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

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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

        public async Task<GroupFriend> GetGroupFriend(int groupId, int otherFriendId, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            GroupFriend result = null;
            try
            {
                Logger.Debug($"GetGroupFriend.BEGIN(groupId={groupId}, otherFriendId={otherFriendId}, useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroupFriends/{App.User.Id}/{groupId}/0/0/{useCache}?{DataModel.Extension.ParamOtherFriendId}={otherFriendId}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    var groupFriends = JsonConvert.DeserializeObject<IEnumerable<GroupFriend>>(jsonString.Result);
                    result = groupFriends.FirstOrDefault();
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

        public async Task<IEnumerable<GroupFriend>> GetGroupFriends(int groupId, bool isActive = true, int top = 0, int skip = 0,
            bool useCache = true, string searchText = null, params string[] arrCatValues)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<GroupFriend> result = null;
            try
            {
                Logger.Debug($"GetGroupFriends.BEGIN(groupId={groupId}, isActive={isActive}, top = {top}, skip = {skip}, useCache={useCache}, searchText={searchText}, arrCatValues.Length={arrCatValues.Length}. {string.Join(", ", arrCatValues)})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetGroupFriends/{App.User.Id}/{groupId}/{top}/{skip}/{useCache}?{DataModel.Extension.ParamIsActive}={isActive}";
                if (!string.IsNullOrEmpty(searchText))
                {
                    requestUrl = $"{requestUrl}&{DataModel.Extension.ParamSearchText}={HttpUtility.UrlEncode(searchText)}";
                }
                if (arrCatValues.Length > 0)
                {
                    for (var i = 0; i < arrCatValues.Length; i++)
                    {
                        requestUrl = $"{requestUrl}&{DataModel.Extension.ParamCategory}{i}={HttpUtility.UrlEncode(arrCatValues[i])}";
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
                Logger.Debug($"GetGroupFriends.END(Count={result?.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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
                //Logger.Debug($"GetMyGroups.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
                Logger.Debug($"GetMyGroups.END(Count={result?.Count() ?? 0}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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

        public async Task<Friend> GetProfile(bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            Friend result = null;
            try
            {
                Logger.Debug($"GetProfile.BEGIN(useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetProfile/{App.User.Id}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<Friend>(jsonString.Result);
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
                Logger.Debug($"GetProfile.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
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

        public async Task<IEnumerable<Notification>> GetNotifications(int top = 0, int skip = 0, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<Notification> result = null;
            try
            {
                Logger.Debug($"GetNotifications.BEGIN(top={top}, skip={skip}, useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetNotifications/{App.User.Id}/{top}/{skip}/{useCache}";
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
                Logger.Debug($"GetNotifications.END(Count={result?.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<bool> ReadNotification(string notifIds)
        {
            var stopWatch = Stopwatch.StartNew();
            var result = false;
            try
            {
                Logger.Debug($"ReadNotification.BEGIN(notifIds={notifIds})");

                Validate();

                var serializedObject = JsonConvert.SerializeObject(
                    new { notifIds = notifIds });

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/ReadNotification/{App.User.Id}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.PutAsync(requestUrl, new StringContent(serializedObject, Encoding.UTF8, "application/json"));

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

        public async Task<Setting> GetSetting(bool useClientCache = true, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            Setting result = null;
            try
            {
                Logger.Debug($"GetSetting.BEGIN(useClientCache={useClientCache}, useCache={useCache})");

                var cachePrefix = $"{CacheTimeoutPrefix}{GetActualAsyncMethodName()}";
                var cacheTimeout = int.Parse(ConfigurationManager.AppSettings[cachePrefix]);
                var cacheKey = $"{cachePrefix}.";
                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useClientCache)
                {
                    result = _memoryCache.Get(cacheKey) as Setting;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Friend/GetSetting/{App.User.Id}/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<Setting>(jsonString.Result);
                }
                else
                {
                    var msg = JsonConvert.DeserializeObject<Message>(jsonString.Result);
                    throw new GoException(msg);
                }

                _memoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
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
                Logger.Debug($"GetSetting.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}