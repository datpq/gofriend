﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using goFriend.Helpers;
using goFriend.Services;
using PCLAppConfig;
using Xamarin.Forms;

namespace goFriend.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        private ObservableCollection<ChatListItemViewModel> _chatListItems = new ObservableCollection<ChatListItemViewModel>();
        public ObservableCollection<ChatListItemViewModel> ChatListItems
        {
            get => _chatListItems;
            set
            {
                _chatListItems = value;
                OnPropertyChanged(nameof(ChatListItems));
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public async Task RefreshCommandAsyncExec()
        {
            try
            {
                Logger.Debug("RefreshCommand.BEGIN");

                IsRefreshing = true;

                var myChats = await App.FriendStore.ChatGetChats();
                foreach (var chat in myChats)
                {
                    if (chat.LogoUrl == null)
                    {
                        chat.LogoUrl = "/logos/group.png";
                    }

                    chat.LogoUrl = $"{ConfigurationManager.AppSettings["HomePageUrl"]}{chat.LogoUrl}";
                    if (ChatListItems.Any(x => x.Chat.Id == chat.Id))
                    {
                        Logger.Debug($"Updating chat {chat.Name}{chat.Id})");
                        var item = ChatListItems.Single(x => x.Chat.Id == chat.Id);
                        item.Chat = chat;
                    }
                    else
                    {
                        Logger.Debug($"Adding new chat {chat.Name}{chat.Id})");
                        ChatListItems.Add(new ChatListItemViewModel {Chat = chat});
                    }
                }

                foreach (var item in ChatListItems.Where(x => myChats.All(y => y.Id != x.Chat.Id)).ToList())
                {
                    Logger.Warn($"Removing chat {item.Chat.Name}{item.Chat.Id})");
                    ChatListItems.Remove(item);
                }

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                IsRefreshing = false;
                Logger.Debug("RefreshCommand.END");
            }
        }

        //public ICommand RefreshCommand => new Command(RefreshCommandAsyncExec);
        public IAsyncCommand RefreshCommand => new AsyncCommand(RefreshCommandAsyncExec, () => !IsRefreshing);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
