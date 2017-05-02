using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr;
using Acr.UserDialogs;
using Humanizer;
using Plugin.HttpTransferTasks;
using Xamarin.Forms;


namespace Sample
{
    public class HttpTaskViewModel : ViewModel, IViewModelLifecycle
    {
        readonly IHttpTask task;
        IDisposable taskSub;


        public HttpTaskViewModel(IHttpTask task)
        {
            this.task = task;
            this.Cancel = new Acr.Command(task.Cancel);
            this.MoreInfo = new Acr.Command(() =>
            {
                if (task.Status == TaskStatus.Error)
                    UserDialogs.Instance.Alert(task.Exception.ToString(), "Error");
            });
        }


        public void OnActivate() => this.taskSub = Observable
            .Create<IHttpTask>(ob =>
            {
                var handler = new PropertyChangedEventHandler((sender, args) => ob.OnNext(this.task));
                this.task.PropertyChanged += handler;
                return () => this.task.PropertyChanged -= handler;
            })
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(x =>
                Device.BeginInvokeOnMainThread(() => this.OnPropertyChanged(String.Empty))
            );

        public void OnDeactivate() => this.taskSub?.Dispose();
        public bool OnBack() => true;


        public string Identifier => this.task.Identifier;
        public bool IsUpload => this.task.IsUpload;
        public TaskStatus Status => this.task.Status;
        public string Uri => this.task.Configuration.Uri;
        public decimal PercentComplete => this.task.PercentComplete;
        public string TransferSpeed => Math.Round(this.task.BytesPerSecond.Bytes().Kilobytes, 2) + " Kb/s";
        public string EstimateMinsRemaining => Math.Round(this.task.EstimatedCompletionTime.TotalMinutes, 1) + " min(s)";

        public ICommand Cancel { get; }
        public ICommand MoreInfo { get; }

        protected virtual void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs args)
            => Device.BeginInvokeOnMainThread(() => this.OnPropertyChanged(String.Empty));
    }
}
