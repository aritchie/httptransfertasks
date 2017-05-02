using System;
using Foundation;


namespace Plugin.HttpTransferTasks
{
    public interface IIosHttpTask : IHttpTask
    {
        void SetStatus(TaskStatus status);
        void SetDownloadComplete(string tempLocation);
        void SetResumeOffset(long resumeOffset, long expectedTotalBytes);
        void SetData(long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite);
        void SetError(NSError error);
    }
}