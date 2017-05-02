using System;


namespace Plugin.HttpTransferTasks
{
    public enum TaskStatus
    {
        Paused,
        PausedByNoNetwork,
        PausedByCostedNetwork,
        Running,
        Resumed,
        Retrying,
        Error,
        Cancelled,
        Completed
    }
}
