using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;


namespace Plugin.HttpTransferTasks
{
    public abstract class AbstractHttpTransferTasks : IHttpTransferTasks
    {
        readonly object syncLock = new object();
        readonly IDictionary<string, IHttpTask> currentTasks = new Dictionary<string, IHttpTask>();


        public virtual IHttpTask Upload(string uri, string localFilePath, bool useMeteredConnection = false)
            => this.Upload(new TaskConfiguration(uri, localFilePath)
            {
                UseMeteredConnection = useMeteredConnection
            });


        public virtual IHttpTask Download(string uri, bool useMeteredConnection = false)
            => this.Download(new TaskConfiguration(uri)
            {
                UseMeteredConnection = useMeteredConnection
            });


        public event EventHandler<TaskListEventArgs> CurrentTasksChanged;
        public abstract IHttpTask Upload(TaskConfiguration config);
        public abstract IHttpTask Download(TaskConfiguration config);


        public IReadOnlyList<IHttpTask> CurrentTasks
        {
            get
            {
                lock (this.syncLock)
                    return new ReadOnlyCollection<IHttpTask>(this.currentTasks.Values.ToList());
            }
        }


        public virtual void CancelAll()
        {
            foreach (var task in this.CurrentTasks)
                task.Cancel();
        }


        protected virtual void Add(IHttpTask task)
        {
            lock (this.syncLock)
            {
                task.PropertyChanged += this.OnTaskPropertyChanged;
                this.currentTasks.Add(task.Identifier, task);
                this.OnChange(task, TaskListChange.Add);
            }
        }


        protected virtual void Remove(IHttpTask task)
        {
            lock (this.syncLock)
            {
                task.PropertyChanged -= this.OnTaskPropertyChanged;
                this.currentTasks.Remove(task.Identifier);
                this.OnChange(task, TaskListChange.Remove);
            }
        }


        protected virtual void OnChange(IHttpTask task, TaskListChange change)
            => this.CurrentTasksChanged?.Invoke(this, new TaskListEventArgs(task, change));


        protected virtual void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(IHttpTask.Status))
                return;

            var task = (IHttpTask) sender;
            switch (task.Status)
            {
                case TaskStatus.Cancelled:
                case TaskStatus.Completed:
                case TaskStatus.Error:
                    this.Remove(task);
                    break;
            }
        }
    }
}
