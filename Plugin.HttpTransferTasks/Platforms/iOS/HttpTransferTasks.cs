using System;
using System.Linq;
using Foundation;


namespace Plugin.HttpTransferTasks
{
    public class HttpTransferTasks : AbstractHttpTransferTasks
    {
        readonly PluginSessionDelegate sessionDelegate;
        readonly NSUrlSessionConfiguration sessionConfig;
        readonly NSUrlSession session;


        public HttpTransferTasks()
        {
            this.sessionDelegate = new PluginSessionDelegate();
            this.sessionConfig = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession");
            this.sessionConfig.HttpMaximumConnectionsPerHost = 1;
            this.session = NSUrlSession.FromConfiguration(
                this.sessionConfig,
                this.sessionDelegate,
                new NSOperationQueue()
            );

            this.session.GetTasks2((_, uploads, downloads) =>
            {
                foreach (NSUrlSessionUploadTask upload in uploads)
                {
                    // TODO: need localFilePath for what WAS uploading
                    // TODO: need to set resumed status
                    this.Add(new HttpTask(this.ToTaskConfiguration(upload), upload));
                    upload.Resume();
                }

                foreach (var download in downloads)
                {
                    this.Add(new HttpTask(this.ToTaskConfiguration(download), download));
                    download.Resume();
                }
            });
        }


        public override IHttpTask Upload(TaskConfiguration config)
        {
            var request = this.CreateRequest(config);
            var native = this.session.CreateUploadTask(request, NSUrl.FromFilename(config.LocalFilePath));
            var task = new HttpTask(config, native);
            this.Add(task);
            native.Resume();

            return task;
        }


        public override IHttpTask Download(TaskConfiguration config)
        {
            var request = this.CreateRequest(config);
            var native = this.session.CreateDownloadTask(request);
            var task = new HttpTask(config, native);
            this.Add(task);
            native.Resume();

            return task;
        }


        protected virtual TaskConfiguration ToTaskConfiguration(NSUrlSessionTask native)
            => new TaskConfiguration(native.OriginalRequest.ToString())
            {
                UseMeteredConnection = native.OriginalRequest.AllowsCellularAccess,
                HttpMethod = native.OriginalRequest.HttpMethod,
                PostData = native.OriginalRequest.Body.ToString(),
                Headers = native.OriginalRequest.Headers.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value.ToString()
                )
            };


        protected override void Add(IHttpTask task)
        {
            if (!(task is IIosHttpTask ios))
                throw new ArgumentException("You must inherit from IIosHttpTask");

            this.sessionDelegate.AddTask(ios);
            base.Add(task);
        }


        protected virtual NSUrlRequest CreateRequest(TaskConfiguration config)
        {
            var url = NSUrl.FromString(config.Uri);
            var request = new NSMutableUrlRequest(url)
            {
                HttpMethod = config.HttpMethod,
                AllowsCellularAccess = config.UseMeteredConnection
            };

            if (!String.IsNullOrWhiteSpace(config.PostData))
                request.Body = NSData.FromString(config.PostData);

            if (config.Headers.Any())
                request.Headers = NSDictionary.FromObjectsAndKeys(
                    config.Headers.Values.ToArray(),
                    config.Headers.Keys.ToArray()
                );

            return request;
        }
    }
}