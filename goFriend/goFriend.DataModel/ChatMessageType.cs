namespace goFriend.DataModel
{
    //The value of Enum is important, don't change the order when you add new value
    public enum ChatMessageType
    {
        Ping = 0,
        JoinChat,
        Text,
        Attachment,
        SysDate, // Date of a bundle of message sent on the same day
        CreateChat, // A new chat group created, or adding removing members
        EditGroup, // For updating a group, disconnecting kicked-out members and connecting new members
        Location,
    }
}
