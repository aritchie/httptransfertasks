using System;
using System.Threading;
using Windows.Networking.BackgroundTransfer;


namespace Plugin.HttpTransferTasks
{
    public class DownloadHttpTask : AbstractUwpHttpTask
    {
        readonly DownloadOperation operation;


        public DownloadHttpTask(TaskConfiguration config, DownloadOperation operation, bool restart) : base(config, false)
        {
            this.operation = operation;
            this.Identifier = this.operation.Guid.ToString();
            this.LocalFilePath = this.operation.ResultFile.Path;

            var task = restart ? this.operation.AttachAsync() : this.operation.StartAsync();
            task.AsTask(
                CancellationToken.None,
                new Progress<DownloadOperation>(x => this.SetData(
                    x.Progress.Status,
                    x.Progress.BytesReceived,
                    x.Progress.TotalBytesToReceive,
                    x.Progress.HasRestarted
                )
            ));
        }


        public override void Cancel()
        {
            this.operation.AttachAsync().Cancel();
            this.Status = TaskStatus.Cancelled;
        }
    }
}
