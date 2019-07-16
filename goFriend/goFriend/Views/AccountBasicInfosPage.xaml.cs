using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountBasicInfosPage : ContentPage
    {
        public AccountBasicInfosPage()
        {
            InitializeComponent();
            LeFirstName.EntryText = App.User.FirstName;
            LeLastName.EntryText = App.User.LastName;
            LeFullName.EntryText = App.User.Name;
            LeEmail.EntryText = App.User.Email;
            DpBirthDay.MaximumDate = new DateTime(DateTime.Today.Year - 10, 1, 1);
            DpBirthDay.MinimumDate = new DateTime(1930, 1, 1);
            DpBirthDay.Date = App.User.Birthday??DateTime.Today;
            LeFirstName.TextChanged += Name_TextChanged;
            LeLastName.TextChanged += Name_TextChanged;
            var labelTextColor = LblBirthDay.TextColor;
            DpBirthDay.Focused += (s, e) => { LblBirthDay.TextColor = (Color)Application.Current.Resources["ColorPrimary"]; };
            DpBirthDay.Unfocused += (s, e) => { LblBirthDay.TextColor = labelTextColor; };

        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            LeFullName.EntryText = LeLastName.EntryText + " " + LeFirstName.EntryText;
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
        }

    }
}