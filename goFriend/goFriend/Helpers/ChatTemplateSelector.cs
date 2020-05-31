using goFriend.DataModel;
using goFriend.Views;
using Xamarin.Forms;

namespace goFriend.Helpers
{
    class ChatTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _incomingTextDataTemplate;
        private readonly DataTemplate _outgoingTextDataTemplate;
        private readonly DataTemplate _outgoingAttachmentDataTemplate;
        private readonly DataTemplate _systemMsgDataTemplate;

        public ChatTemplateSelector()
        {
            _incomingTextDataTemplate = new DataTemplate(typeof(ChatIncomingViewCell));
            _outgoingTextDataTemplate = new DataTemplate(typeof(ChatOutgoingViewCell));
            _outgoingAttachmentDataTemplate = new DataTemplate(typeof(ChatOutgoingAttachmentViewCell));
            _systemMsgDataTemplate = new DataTemplate(typeof(ChatSystemMsgViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var chatMessage = item as ChatMessage;
            if (chatMessage == null)
                return null;

            if (chatMessage.IsSystemMessage)
            {
                return _systemMsgDataTemplate;
            }
            if (chatMessage.IsOwnMessage)
            {
                if (chatMessage.MessageType == ChatMessageType.Text)
                {
                    return _outgoingTextDataTemplate;
                }
                if (chatMessage.MessageType == ChatMessageType.Attachment)
                {
                    return _outgoingAttachmentDataTemplate;
                }
            }
            else
            {
                if (chatMessage.MessageType == ChatMessageType.Text)
                {
                    return _incomingTextDataTemplate;
                }
                if (chatMessage.MessageType == ChatMessageType.Attachment)
                {
                    return _incomingTextDataTemplate;
                }
            }
            return null;
        }
    }
}
