﻿namespace goFriend.DataModel
{
    public class ChatJoinChatModel : IChatMessage
    {
        public int ChatId { get; set; }

        public ChatMessageType MessageType { get; set; } = ChatMessageType.JoinChat;
        public int OwnerId { get; set; }
        public string Token { get; set; }
    }
}
