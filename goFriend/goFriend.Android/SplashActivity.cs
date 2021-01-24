
using Android.App;
using Android.OS;

namespace goFriend.Droid
{
    [Activity(Label = "@string/app_label", Theme = "@style/splashTheme", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            StartActivity(typeof(MainActivity));
        }
    }
}