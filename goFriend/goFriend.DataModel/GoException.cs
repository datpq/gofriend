using System;

namespace goFriend.DataModel
{
    public class GoException : Exception
    {
        public Message Msg { get; }

        public GoException(Message msg) : base(msg.ToString())
        {
            Msg = msg;
        }
    }
}
