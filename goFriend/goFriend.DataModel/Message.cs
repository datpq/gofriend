namespace goFriend.DataModel
{
    public enum MessageCode
    {
        Unknown = 12976,
        InvalidState,
        FacebookIdNull,
        IdOrEmailNull,
        UserNotFound
    }

    public class Message
    {
        public MessageCode Code { get; set; }
        public string Msg { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Msg}";
        }
    }
}
