using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Plugin.HttpTransferTasks;
using Xamarin.Forms;


namespace Sample
{
    public class MainViewModel : ViewModel
    {
        public MainViewModel()
        {
            this.NewTask = new Command(async () =>
                await App.Current.MainPage.Navigation.PushAsync(new NewTaskPage())
            );
            this.MoreInfo = new Command<HttpTaskViewModel>(x => x.MoreInfo.Execute(null));
            this.CancelAll = new Command(CrossHttpTransfers.Current.CancelAll);
            this.Tasks = new ObservableCollection<HttpTaskViewModel>();

            CrossHttpTransfers.Current.CurrentTasksChanged += (sender, args) =>
            {
                if (args.Change == TaskListChange.Add)
                    Device.BeginInvokeOnMainThread(() => this.Tasks.Insert(0, new HttpTaskViewModel(args.Task)));
            };
        }


        public ICommand NewTask { get; }
        public ICommand MoreInfo { get; }
        public ICommand CancelAll { get; }
        public ObservableCollection<HttpTaskViewModel> Tasks { get; }
    }
}
