using goFriend.DataModel;
using goFriend.Views;
using Xamarin.Forms;

namespace goFriend.Helpers
{
    class ChatTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _incomingDataTemplate;
        private readonly DataTemplate _outgoingDataTemplate;
        private readonly DataTemplate _systemMsgDataTemplate;

        public ChatTemplateSelector()
        {
            _incomingDataTemplate = new DataTemplate(typeof(ChatIncomingViewCell));
            _outgoingDataTemplate = new DataTemplate(typeof(ChatOutgoingViewCell));
            _systemMsgDataTemplate = new DataTemplate(typeof(ChatSystemMsgViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var chatMessage = item as ChatMessage;
            if (chatMessage == null)
                return null;

            return chatMessage.IsSystemMessage ? _systemMsgDataTemplate
                : chatMessage.IsOwnMessage ? _outgoingDataTemplate : _incomingDataTemplate;
        }

    }
}
