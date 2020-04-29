using System.Collections.Generic;

namespace goFriend.DataModel
{
    public class ChatJoinChatModel : IChatMessage
    {
        public int ChatId { get; set; }
        public int LastMsgIndex { get; set; }
        public int MissingMsgCount { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<ChatMessage> ChatMessages { get; set; }

        public ChatMessageType MessageType { get; set; } = ChatMessageType.JoinChat;
    }
}
