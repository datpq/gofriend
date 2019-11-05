using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using goFriend.DataModel;
using goFriend.Services;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace goFriend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrowsePage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public BrowsePage()
        {
            InitializeComponent();
            PickerGroups.Title = $"{res.Select} {res.Groups}";
            LblGroup.Text = $"{res.Groups}:";

            try
            {
                UserDialogs.Instance.ShowLoading(res.Processing);
                App.TaskGetMyGroups.Wait();
                PickerGroups.ItemsSource = App.MyGroups.Where(x => x.GroupFriend.Active).OrderBy(x => x.Group.Name).ToList();
                if (PickerGroups.Items.Count > 0)
                {
                    PickerGroups.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void PickerGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Logger.Debug($"SelectedIndex={PickerGroups.SelectedIndex}, SelectedItem={PickerGroups.SelectedItem}");
            if (!(PickerGroups.SelectedItem is ApiGetGroupsModel selectedGroup)) return;
            var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) > 0).ToList();
            foreach (var child in rowToRemove)
            {
                Grid.Children.Remove(child);
            }
            var groupFixedCatValues = await App.FriendStore.GetGroupFixedCatValues(selectedGroup.Group.Id);
            var arrCats = selectedGroup.Group.GetCatDescList().ToList();
            var arrFixedCats = groupFixedCatValues.GetCatList().ToList();
            var arrPickers = new Picker[arrCats.Count() - arrFixedCats.Count];
            for (var i = 0; i < arrCats.Count() - arrFixedCats.Count(); i++)
            {
                //Logger.Debug($"i={i}");
                var lbl = new Label
                {
                    FontSize = LblGroup.FontSize,
                    VerticalOptions = LblGroup.VerticalOptions,
                    TextColor = LblGroup.TextColor,
                    Text = $"{arrCats[i + arrFixedCats.Count()]}:"
                };
                Grid.SetColumn(lbl, 0);
                Grid.SetRow(lbl, i + 1);
                Grid.Children.Add(lbl);
                var picker = new Picker
                {
                    FontSize = PickerGroups.FontSize,
                    ItemDisplayBinding = new Binding("CatValue"),
                    Title = $"{res.Select} {arrCats[i + arrFixedCats.Count()]}"
                };
                arrPickers[i] = picker;
                if (i == 0)
                {
                    await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id).ContinueWith(task =>
                        {
                            picker.ItemsSource = task.Result.ToList();
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                var localI = i;
                picker.SelectedIndexChanged += async (o, args) =>
                {
                    //Logger.Debug($"localI={localI}");
                    for (var j = localI + 1; j < arrPickers.Length; j++)
                    {
                        arrPickers[j].ItemsSource = null;
                        //Logger.Debug($"j={j}");
                        if (j == localI + 1)
                        {
                            var arrCatValues = new string[j];
                            for (var k = 0; k < j; k++)
                            {
                                //Logger.Debug($"arrPickers[{k}].SelectedItem={arrPickers[k].SelectedItem}");
                                arrCatValues[k] = (arrPickers[k].SelectedItem as ApiGetGroupCatValuesModel)?.CatValue;
                            }
                            Logger.Debug($"arrCatValues={JsonConvert.SerializeObject(arrCatValues)}");
                            await App.FriendStore.GetGroupCatValues(selectedGroup.Group.Id, true, arrCatValues).ContinueWith(task =>
                            {
                                arrPickers[j].ItemsSource = task.Result.ToList();
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    }
                };
                Grid.SetColumn(picker, 1);
                Grid.SetRow(picker, i + 1);
                Grid.Children.Add(picker);
            }
        }
    }
}