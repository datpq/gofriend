using Acr.UserDialogs;
using goFriend.Services;
using goFriend.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountDetailPage : ContentPage
    {
        private AccountBasicInfosViewModel _viewModel;
        private static readonly ILogger Logger = new LoggerNLogPclImpl(NLog.LogManager.GetCurrentClassLogger());

        public AccountDetailPage(AccountBasicInfosViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        private async void CmdSave_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Email))
            {
                App.DisplayMsgError(string.Format(res.MsgMandatoryField, res.Email.RemoveEndingMark()));
                TxtEmail.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(_viewModel.Phone))
            {
                App.DisplayMsgError(string.Format(res.MsgMandatoryField, res.Tel.RemoveEndingMark()));
                TxtPhone.Focus();
                return;
            }
            if (!await App.DisplayMsgQuestion(res.MsgSaveConfirm)) return;
            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                Logger.Debug("CmdSave_Click.BEGIN");

                var newUser = new DataModel.Friend
                {
                    Id = App.User.Id,
                    Email = _viewModel.Email,
                    Relationship = _viewModel.Relationship,
                    Phone = _viewModel.Phone,
                    Gender = _viewModel.Gender,
                    Birthday = _viewModel.Birthday
                };

                var result = await App.FriendStore.SaveBasicInfo(newUser);
                if (result)
                {
                    App.User.Email = newUser.Email;
                    App.User.Relationship = newUser.Relationship;
                    App.User.Phone = newUser.Phone;
                    App.User.Gender = newUser.Gender;
                    App.User.Birthday = newUser.Birthday;
                    Settings.LastUser = App.User;
                    App.DisplayMsgInfo(res.SaveSuccess);
                }
                else
                {
                    Logger.Error("Saving failed.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("CmdSave_Click.END");
            }
        }
    }
}