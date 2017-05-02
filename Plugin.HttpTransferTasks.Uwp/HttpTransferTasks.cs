using System;
using System.IO;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;


namespace Plugin.HttpTransferTasks
{
    public class HttpTransferTasks : AbstractHttpTransferTasks
    {
        public HttpTransferTasks()
        {
            BackgroundUploader
                .GetCurrentUploadsAsync()
                .AsTask()
                .ContinueWith(result =>
                {
                    foreach (var task in result.Result)
                    {
                        var config = new TaskConfiguration(task.RequestedUri.ToString(), task.SourceFile.Path)
                        {
                            HttpMethod = task.Method,
                            UseMeteredConnection = task.CostPolicy != BackgroundTransferCostPolicy.UnrestrictedOnly
                        };
                        this.Add(new UploadHttpTask(config, task, true));
                    }
                });

            BackgroundDownloader
                .GetCurrentDownloadsAsync()
                .AsTask()
                .ContinueWith(result =>
                {
                    foreach (var task in result.Result)
                    {
                        var config = new TaskConfiguration(task.RequestedUri.ToString())
                        {
                            HttpMethod = task.Method,
                            UseMeteredConnection = task.CostPolicy != BackgroundTransferCostPolicy.UnrestrictedOnly
                        };
                        this.Add(new DownloadHttpTask(config, task, true));
                    }
                });
        }


        public override IHttpTask Upload(TaskConfiguration config)
        {

            if (String.IsNullOrWhiteSpace(config.LocalFilePath))
                throw new ArgumentException("You must set the local file path when uploading");

            if (!File.Exists(config.LocalFilePath))
                throw new ArgumentException($"File '{config.LocalFilePath}' does not exist");

            var task = new BackgroundUploader
            {
                Method = config.HttpMethod,
                CostPolicy = config.UseMeteredConnection
                    ? BackgroundTransferCostPolicy.Default
                    : BackgroundTransferCostPolicy.UnrestrictedOnly
            };

            foreach (var header in config.Headers)
                task.SetRequestHeader(header.Key, header.Value);

            // seriously - this should not be async!
            var file = StorageFile.GetFileFromPathAsync(config.LocalFilePath).AsTask().Result;
            var operation = task.CreateUpload(new Uri(config.Uri), file);
            var httpTask = new UploadHttpTask(config, operation, false);
            this.Add(httpTask);

            return httpTask;
        }


        public override IHttpTask Download(TaskConfiguration config)
        {
            var task = new BackgroundDownloader
            {
                Method = config.HttpMethod,
                CostPolicy = config.UseMeteredConnection
                    ? BackgroundTransferCostPolicy.Default
                    : BackgroundTransferCostPolicy.UnrestrictedOnly
            };
            foreach (var header in config.Headers)
                task.SetRequestHeader(header.Key, header.Value);

            var filePath = config.LocalFilePath ?? Path.Combine(ApplicationData.Current.LocalFolder.Path, Path.GetRandomFileName());
            var fileName = Path.GetFileName(filePath);
            var directory = Path.GetDirectoryName(filePath);

            // why are these async??
            var folder = StorageFolder.GetFolderFromPathAsync(directory).AsTask().Result;
            var file = folder.CreateFileAsync(fileName).AsTask().Result;

            var operation = task.CreateDownload(new Uri(config.Uri), file);
            var httpTask = new DownloadHttpTask(config, operation, false);
            this.Add(httpTask);

            return httpTask;
        }
    }
}