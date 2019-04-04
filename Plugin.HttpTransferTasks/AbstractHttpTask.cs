using System;


namespace Plugin.HttpTransferTasks
{
    public abstract class AbstractHttpTask : AbstractNotifyPropertyChanged, IHttpTask
    {
        protected AbstractHttpTask(TaskConfiguration config, bool upload)
        {
            this.Configuration = config;
            this.LocalFilePath = config.LocalFilePath;
            this.IsUpload = upload;
        }


        protected virtual void RunCalculations()
        {
			if (this.FileSize <= 0 || this.BytesTransferred <= 0)
				return;

			var raw = ((decimal)this.BytesTransferred / (decimal)this.FileSize) * 100;
			this.PercentComplete = Math.Round(raw, 2);

            var elapsedTime = DateTime.Now - this.StartTime;
			this.BytesPerSecond = this.BytesTransferred / elapsedTime.TotalSeconds;

			var rawEta = this.FileSize / this.BytesPerSecond;
			this.EstimatedCompletionTime = TimeSpan.FromSeconds(rawEta);
		}


        public abstract void Cancel();


        public TaskConfiguration Configuration { get; }


        string id;
        public string Identifier
        {
            get => this.id;
            protected set => this.SetProperty(ref this.id, value);
        }


        public bool IsUpload { get; }


        TaskStatus status;
        public TaskStatus Status
        {
            get => this.status;
            protected set => this.SetProperty(ref this.status, value);
        }


        string remoteFile;
        public string RemoteFileName
        {
            get => this.remoteFile;
            protected set => this.SetProperty(ref this.remoteFile, value);
        }


        string localFile;
        public string LocalFilePath
        {
            get => this.localFile;
            protected set => this.SetProperty(ref this.localFile, value);
        }


        long resumeOffset;
        public long ResumeOffset
        {
            get => this.resumeOffset;
            protected set => this.SetProperty(ref this.resumeOffset, value);
        }


        long fileSize;
        public long FileSize
        {
            get => this.fileSize;
            protected set => this.SetProperty(ref this.fileSize, value);
        }


        long bytesXfer;
        public long BytesTransferred
        {
            get => this.bytesXfer;
            protected set => this.SetProperty(ref this.bytesXfer, value);
        }


        decimal percentComplete;
        public decimal PercentComplete
        {
            get => this.percentComplete;
            protected set => this.SetProperty(ref this.percentComplete, value);
        }


        Exception exception;
        public Exception Exception
        {
            get => this.exception;
            set
            {
                this.exception = value;
                this.Status = TaskStatus.Error;
                this.OnPropertyChanged();
            }
        }

        double bytesPerSecond;
        public double BytesPerSecond
        {
            get => this.bytesPerSecond;
            protected set => this.SetProperty(ref this.bytesPerSecond, value);
        }


        TimeSpan ctime;
        public TimeSpan EstimatedCompletionTime
        {
            get => this.ctime;
            protected set => this.SetProperty(ref this.ctime, value);
        }


        public DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;
    }
}
