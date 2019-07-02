namespace goFriend.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public AccountViewModel()
        {
            Title = res.Account;
        }

        public string Avatar => App.User.Avatar;
    }
}
