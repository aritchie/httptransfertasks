using System;
using System.ComponentModel;
using Acr.Notifications;
using Xamarin.Forms;
using Plugin.HttpTransferTasks;
using System.Diagnostics;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]


namespace Sample
{
    public partial class App : Application
    {
        const string NOT_TITLE = "HTTP Transfers";


        public App()
        {
            this.InitializeComponent();
            this.MainPage = new NavigationPage(new MainPage());
        }


        protected override void OnStart()
        {
            base.OnStart();

            CrossHttpTransfers.Current.CurrentTasksChanged += (sender, args) =>
            {
                switch (args.Change)
                {
                    case TaskListChange.Add:
                        args.Task.PropertyChanged += this.OnTaskPropertyChanged;
                        break;

                    case TaskListChange.Remove:
                        args.Task.PropertyChanged -= this.OnTaskPropertyChanged;
                        break;
                }
            };
        }


        protected override void OnResume()
        {
            base.OnResume();
            CrossNotifications.Current.Badge = 0;
        }


        void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(IHttpTask.Status))
                return;

            var task = (IHttpTask) sender;
            var type = task.IsUpload ? "Upload" : "Download";

            Device.BeginInvokeOnMainThread(() =>
            {
                var cn = CrossNotifications.Current;
                if (task.Status == TaskStatus.Error)
                {
                    Debug.WriteLine(task.Exception.ToString());
                    cn.Send(
                        NOT_TITLE,
                        $"[ERROR] {task.Configuration.Uri} - {task.Exception}"
                    );
                }
                else
                {
                    cn.Send(
                        NOT_TITLE,
                        $"{type} of {task.RemoteFileName} to {task.Configuration.Uri} finished with status: {task.Status}"
                    );
                    cn.Badge += 1;
                }
            });
        }
    }
}
