using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using goFriend.Services;
using UIKit;
using goFriend.AppleSignIn;
#if __IOS__13
using AuthenticationServices;
#endif


#if __IOS__13
[assembly: Xamarin.Forms.Dependency(typeof(goFriend.iOS.Services.AppleSignInServiceiOS))]
#endif

namespace goFriend.iOS.Services
{
    public class AppleSignInServiceiOS : IAppleSignInService
    {
#if __IOS__13
		AuthManager authManager;
#endif

        bool Is13 => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
        WebAppleSignInService webSignInService;

        public AppleSignInServiceiOS()
        {
            if (!Is13)
                webSignInService = new WebAppleSignInService();
        }

        public async Task<AppleAccount> SignInAsync()
        {
            // Fallback to web for older iOS versions
            if (!Is13)
                return await webSignInService.SignInAsync();

            AppleAccount appleAccount = default;

#if __IOS__13
			var provider = new ASAuthorizationAppleIdProvider();
			var req = provider.CreateRequest();

			authManager = new AuthManager(UIApplication.SharedApplication.KeyWindow);

			req.RequestedScopes = new[] { ASAuthorizationScope.FullName, ASAuthorizationScope.Email };
			var controller = new ASAuthorizationController(new[] { req });

			controller.Delegate = authManager;
			controller.PresentationContextProvider = authManager;

			controller.PerformRequests();

			var creds = await authManager.Credentials;

			if (creds == null)
				return null;

			appleAccount = new AppleAccount();
			appleAccount.IdToken = JwtToken.Decode(new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString());
			appleAccount.Email = creds.Email;
			appleAccount.UserId = creds.User;
			appleAccount.Name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, NSPersonNameComponentsFormatterOptions.Phonetic);
			//DPH
			appleAccount.FirstName = creds.FullName?.GivenName ?? string.Empty;
            appleAccount.LastName = creds.FullName?.FamilyName ?? string.Empty;
            appleAccount.MiddleName = creds.FullName?.MiddleName ?? string.Empty;
            if (appleAccount.Name == string.Empty)
            {
                appleAccount.Name = creds.FullName?.Nickname ??
                                    (appleAccount.FirstName == string.Empty ? string.Empty : $"{appleAccount.LastName} {appleAccount.FirstName}");
			}
			appleAccount.RealUserStatus = creds.RealUserStatus.ToString();
#endif

            return appleAccount;
        }

        public bool Callback(string url) => true;
    }

#if __IOS__13
	class AuthManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
	{
		public Task<ASAuthorizationAppleIdCredential> Credentials
			=> tcsCredential?.Task;

		TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;

		UIWindow presentingAnchor;

		public AuthManager(UIWindow presentingWindow)
		{
			tcsCredential = new TaskCompletionSource<ASAuthorizationAppleIdCredential>();
			presentingAnchor = presentingWindow;
		}

		public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
			=> presentingAnchor;

		[Export("authorizationController:didCompleteWithAuthorization:")]
		public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
		{
			var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
			tcsCredential?.TrySetResult(creds);
		}

		[Export("authorizationController:didCompleteWithError:")]
		public void DidComplete(ASAuthorizationController controller, NSError error)
			=> tcsCredential?.TrySetException(new Exception(error.LocalizedDescription));
	}
#endif
}