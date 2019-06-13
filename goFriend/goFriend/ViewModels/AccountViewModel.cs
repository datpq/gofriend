namespace goFriend.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public AccountViewModel()
        {
            Title = res.Account;
        }

        public string Avatar
        {
            get
            {
                return App.User.Avatar;
            }
        }
    }
}
