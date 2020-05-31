namespace goFriend.DataModel
{
    public enum ChatMessageType
    {
        Ping = 0,
        JoinChat,
        Text,
        Attachment,
        SysDate, // Date of a bundle of message sent on the same day
    }
}
