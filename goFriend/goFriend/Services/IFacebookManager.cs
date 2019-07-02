using goFriend.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace goFriend.Services
{
    public interface IFacebookManager
    {
        void Logout();
        bool IsLoggedIn();
    }
}
