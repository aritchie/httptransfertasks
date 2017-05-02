using System;
using Windows.Networking.BackgroundTransfer;


namespace Plugin.HttpTransferTasks
{
    public abstract class AbstractUwpHttpTask : AbstractHttpTask
    {
        protected AbstractUwpHttpTask(TaskConfiguration config, bool isUpload) : base(config, isUpload)
        {
        }


        protected virtual void SetData(BackgroundTransferStatus status, ulong bytesXfer, ulong totalBytesToXfer, bool hasRestarted)
        {
            switch (status)
            {
                case BackgroundTransferStatus.PausedByApplication:
                    break;

                case BackgroundTransferStatus.Canceled:
                    this.Status = TaskStatus.Cancelled;
                    break;

                case BackgroundTransferStatus.Completed:
                    break;
            }
            this.BytesTransferred = (long)bytesXfer;
            this.FileSize = (long)totalBytesToXfer;
            this.RunCalculations();
        }
    }
}
