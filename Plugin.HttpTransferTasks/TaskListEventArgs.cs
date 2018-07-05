using System;


namespace Plugin.HttpTransferTasks
{
    public class TaskListEventArgs : EventArgs
    {
        public TaskListEventArgs(IHttpTask task, TaskListChange change)
        {
            this.Task = task;
            this.Change = change;
        }


        public IHttpTask Task { get; }
        public TaskListChange Change { get; }
    }
}
