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
                case BackgroundTransferStatus.PausedSystemPolicy:
                    this.Status = TaskStatus.Paused;
                    break;

                case BackgroundTransferStatus.PausedCostedNetwork:
                    this.Status = TaskStatus.PausedByNoNetwork;
                    break;

                case BackgroundTransferStatus.PausedByApplication:
                    this.Status = TaskStatus.Paused;
                    break;

                case BackgroundTransferStatus.Error:
                    // TODO: get exception
                    break;

                case BackgroundTransferStatus.Canceled:
                    this.Status = TaskStatus.Cancelled;
                    break;

                case BackgroundTransferStatus.Completed:
                    this.Status = TaskStatus.Completed;
                    break;

                case BackgroundTransferStatus.Running:
                    this.Status = TaskStatus.Running;
                    break;
            }
            this.BytesTransferred = (long)bytesXfer;
            this.FileSize = (long)totalBytesToXfer;
            this.RunCalculations();
        }
    }
}
