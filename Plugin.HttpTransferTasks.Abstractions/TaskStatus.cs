using System;


namespace Plugin.HttpTransferTasks
{
    public enum TaskStatus
    {
        Running,
        Resumed,
        Retrying,
        Error,
        Cancelled,
        Completed
    }
}
