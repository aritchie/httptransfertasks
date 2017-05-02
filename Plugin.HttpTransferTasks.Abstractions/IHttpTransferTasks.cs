using System;
using System.Collections.Generic;


namespace Plugin.HttpTransferTasks
{
    public interface IHttpTransferTasks
    {
        // TODO: when bad response or security challenge, need a way to set new token on header
        IReadOnlyList<IHttpTask> CurrentTasks { get; }
        event EventHandler<TaskListEventArgs> CurrentTasksChanged;

        IHttpTask Upload(string uri, string localFilePath = null, bool useMeteredConnection = false);
        IHttpTask Upload(TaskConfiguration config);

        IHttpTask Download(string uri, bool useMeteredConnection = false);
        //IHttpTask Download(string uri, string localFilePathWhereToSave = null, bool useMeteredConnection = false);
        IHttpTask Download(TaskConfiguration config);

        void CancelAll();
    }
}
