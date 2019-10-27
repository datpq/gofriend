﻿namespace goFriend.DataModel
{
    public enum MessageCode
    {
        Unknown = 12976,
        MissingToken,
        InvalidState,
        FacebookIdNull,
        IdOrEmailNull,
        UserNotFound,
        GroupNotFound,
        UserTokenError,
        InvalidData,
        NoInternet
    }

    public class Message
    {
        public static readonly Message MsgMissingToken = new Message { Code = MessageCode.MissingToken, Msg = "Missing Token" };
        public static readonly Message MsgInvalidState = new Message { Code = MessageCode.InvalidState, Msg = "Invalid State" };
        public static readonly Message MsgFacebookIdNull = new Message { Code = MessageCode.FacebookIdNull, Msg = "FacebookId is null" };
        public static readonly Message MsgIdOrEmailNull = new Message { Code = MessageCode.IdOrEmailNull, Msg = "Id or Email is null" };
        public static readonly Message MsgUnknown = new Message { Code = MessageCode.Unknown, Msg = "Unknown error" };
        public static readonly Message MsgUserNotFound = new Message { Code = MessageCode.UserNotFound, Msg = "User not found." };
        public static readonly Message MsgGroupNotFound = new Message { Code = MessageCode.GroupNotFound, Msg = "Group not found." };
        public static readonly Message MsgWrongToken = new Message { Code = MessageCode.UserTokenError, Msg = "Wrong Token" };
        public static readonly Message MsgInvalidData = new Message { Code = MessageCode.InvalidData, Msg = "Invalid data" };
        public static readonly Message MsgNoInternet = new Message { Code = MessageCode.NoInternet, Msg = "No internet" };

        public MessageCode Code { get; set; }
        public string Msg { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Msg}";
        }
    }
}
