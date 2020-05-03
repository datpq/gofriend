namespace goFriend.DataModel
{
    public interface IChatMessage
    {
        ChatMessageType MessageType { get; set;  }
        int  OwnerId { get; set; }
        string Token { get; set; }
    }

    public enum ChatMessageType
    {
        Ping = 0,
        JoinChat,
        Text,
        SysDate, // Date of a bundle of message sent on the same day
    }
}
