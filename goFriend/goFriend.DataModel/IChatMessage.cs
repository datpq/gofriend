namespace goFriend.DataModel
{
    public interface IChatMessage
    {
        ChatMessageType MessageType { get; set;  }
    }

    public enum ChatMessageType
    {
        Ping = 0,
        JoinChat,
        SendMessage
    }
}
