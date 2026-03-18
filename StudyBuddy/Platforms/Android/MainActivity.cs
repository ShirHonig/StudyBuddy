using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Bundled.Platforms.Android;
using Firebase.Crashlytics;

namespace StudyBuddy
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FirebaseCrashlytics.Instance.SetCrashlyticsCollectionEnabled(false);

            CrossFirebase.Initialize(this, null, new CrossFirebaseSettings(
                isAuthEnabled: true,
                isFirestoreEnabled: false,
                isCloudMessagingEnabled: false
            ));
        }
    }
}
