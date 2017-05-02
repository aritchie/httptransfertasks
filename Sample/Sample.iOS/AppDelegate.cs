using System;
using Foundation;
using UIKit;
using Xamarin.Forms;


namespace Sample.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.Init();
            this.LoadApplication(new App());
            new Acr.XamForms.ListView(); // fix linker

            return base.FinishedLaunching(app, options);
        }
    }
}
