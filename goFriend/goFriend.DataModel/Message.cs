namespace goFriend.DataModel
{
    public enum MessageCode
    {
        Unknown = 12976,
        MissingToken,
        InvalidState,
        FacebookIdNull,
        IdOrEmailNull,
        UserNotFound,
        UserTokenError
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
