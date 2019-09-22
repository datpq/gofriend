using System;
using goFriend.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountBasicInfosPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public AccountBasicInfosPage()
        {
            InitializeComponent();
            LeFirstName.EntryText = App.User.FirstName;
            LeLastName.EntryText = App.User.LastName;
            LeFullName.EntryText = App.User.Name;
            LeFullName.IsEnabled = false;
            LeEmail.EntryText = App.User.Email;
            LeEmail.IsEnabled = string.IsNullOrEmpty(App.User.Email);
            DpBirthDay.MaximumDate = new DateTime(DateTime.Today.Year - 10, 1, 1);
            DpBirthDay.MinimumDate = new DateTime(1930, 1, 1);
            DpBirthDay.Date = App.User.Birthday??DateTime.Today;
            LeFirstName.TextChanged += Name_TextChanged;
            LeLastName.TextChanged += Name_TextChanged;
            PkGender.Items.Add(res.Male);
            PkGender.Items.Add(res.Female);
            PkGender.SelectedIndex = App.User.Gender == null ? -1 : App.User.Gender == "male" ? 0 : 1;
            var labelTextColor = LblBirthDay.TextColor;
            DpBirthDay.Focused += (s, e) => { LblBirthDay.TextColor = (Color)Application.Current.Resources["ColorPrimary"]; };
            DpBirthDay.Unfocused += (s, e) => { LblBirthDay.TextColor = labelTextColor; };
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            LeFullName.EntryText = LeLastName.EntryText + " " + LeFirstName.EntryText;
        }

        private async void CmdSave_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug("CmdSave_Click.BEGIN");
                var oldFirstName = App.User.FirstName;
                var oldLastName = App.User.LastName;
                var oldName = App.User.Name;
                var oldEmail = App.User.Email;
                var oldBirthDay = App.User.Birthday;
                var oldGender = App.User.Gender;
                App.User.FirstName = LeFirstName.EntryText;
                App.User.LastName = LeLastName.EntryText;
                App.User.Name = LeFullName.EntryText;
                App.User.Email = LeEmail.EntryText;
                App.User.Birthday = DpBirthDay.Date;
                App.User.Gender = PkGender.SelectedIndex == -1 ? null : PkGender.SelectedIndex == 0 ? "male" : "female";
                Logger.Debug("Calling SaveBasicInfo...");
                var result = await App.FriendStore.SaveBasicInfo(App.User);
                if (result)
                {
                    Settings.LastUser = App.User;
                    App.DisplayMsgInfo(res.SaveSuccess);
                }
                else
                {
                    Logger.Error("Saving failed.");
                    App.User.FirstName = oldFirstName;
                    App.User.LastName = oldLastName;
                    App.User.Name = oldName;
                    App.User.Email = oldEmail;
                    App.User.Birthday = oldBirthDay;
                    App.User.Gender = oldGender;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                Logger.Debug("CmdSave_Click.END");
            }
        }
    }
}