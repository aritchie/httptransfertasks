using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


namespace Plugin.HttpTransferTasks
{
    public class HttpTask : AbstractHttpTask
    {
        readonly HttpClient httpClient;
        readonly CancellationTokenSource cancelSrc;


        public HttpTask(TaskConfiguration config, bool upload, string identifier) : base(config, upload)
        {
            this.LocalFilePath = config.LocalFilePath;
            this.Identifier = identifier;

            this.httpClient = new HttpClient();
            this.cancelSrc = new CancellationTokenSource();
            this.Identifier = identifier;
            this.Run();
        }


        Task Run() => Task.Run(async () =>
        {
            if (this.IsUpload)
            {
                await this.DoUpload().ConfigureAwait(false);
            }
            else
            {
                await this.DoDownload().ConfigureAwait(false);
            }
        });


        public override void Cancel()
        {
            this.Status = TaskStatus.Cancelled;
            this.cancelSrc.Cancel();
            this.httpClient.CancelPendingRequests();
        }


        async Task DoUpload()
        {
            var file = new FileInfo(this.LocalFilePath);
            this.FileSize = file.Length;
            this.LocalFilePath = file.FullName;
            this.RemoteFileName = file.Name;

            while (this.Status != TaskStatus.Completed && !this.cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var content = new MultipartFormDataContent();
                    content.Add(
                        new ProgressStreamContent(
                            file.OpenRead(),
                            8192,
                            sent =>
                            {
                                this.Status = TaskStatus.Running;
                                this.BytesTransferred += sent;
                                this.RunCalculations();
                            }
                        ),
                        "blob",
                        this.RemoteFileName
                    );

                    await this.httpClient.PostAsync(this.Configuration.Uri, content, this.cancelSrc.Token);
                    this.Status = this.cancelSrc.IsCancellationRequested
                        ? TaskStatus.Cancelled
                        : TaskStatus.Completed;
                }
                catch (TimeoutException)
                {
                    this.Status = TaskStatus.Retrying;
                }
                catch (IOException ex)
                {
                    if (ex.InnerException is WebException)
                    {
                        this.Status = TaskStatus.Retrying;
                    }
                    else
                    {
                        this.Exception = ex;
                    }
                }
                catch (TaskCanceledException)
                {
                    this.Status = this.cancelSrc.IsCancellationRequested
                        ? TaskStatus.Cancelled
                        : TaskStatus.Retrying;
                }
                catch (Exception ex)
                {
                    this.Exception = ex;
                }
            }
        }


        async Task DoDownload()
        {
            var file = new FileInfo(this.LocalFilePath);

            var fileMode = FileMode.CreateNew;
            if (file.Exists)
            {
                this.ResumeOffset = file.Length + 1;
                this.BytesTransferred = file.Length;
                fileMode = FileMode.Append;
            }

            using (var fs = file.Open(fileMode, FileAccess.Write, FileShare.Write))
            {
                while (this.Status != TaskStatus.Completed && !this.cancelSrc.IsCancellationRequested)
                {
                    try
                    {
                        //this.httpClient.DefaultRequestHeaders.ExpectContinue = false;
                        this.httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                        this.httpClient.DefaultRequestHeaders.Add("Keep-Alive", "false");

                        await this.DoDownload(fs);
                        this.Status = this.cancelSrc.IsCancellationRequested
                            ? TaskStatus.Cancelled
                            : TaskStatus.Completed;
                    }
                    catch (TaskCanceledException)
                    {
                        this.Status = this.cancelSrc.IsCancellationRequested
                            ? TaskStatus.Cancelled
                            : TaskStatus.Retrying;
                    }
                    catch (IOException ex)
                    {
                        if (ex.InnerException is WebException)
                        {
                            this.Status = TaskStatus.Retrying;
                        }
                        else
                        {
                            this.Exception = ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Exception = ex;
                    }
                }
            }
        }


        async Task DoDownload(FileStream fs)
        {
            var buffer = new byte[8192];
            if (this.BytesTransferred > 0)
            {
                this.ResumeOffset = this.BytesTransferred + 1;
                this.httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(this.ResumeOffset, null);
            }
            var response = await this.httpClient.GetAsync(
                this.Configuration.Uri,
                HttpCompletionOption.ResponseHeadersRead,
                this.cancelSrc.Token
            );
            response.EnsureSuccessStatusCode();

            var inputStream = await response.Content.ReadAsStreamAsync();
            var read = inputStream.Read(buffer, 0, buffer.Length);

            if (response.Headers.AcceptRanges == null && fs.Length > 0)
            {
                // resume not supported, starting over
                fs.SetLength(0);
                fs.Flush();
                this.BytesTransferred = 0;
                this.ResumeOffset = 0;
            }

            while (read > 0 && !this.cancelSrc.IsCancellationRequested)
            {
                this.FileSize = response.Content.Headers?.ContentRange?.Length ?? response.Content?.Headers?.ContentLength ?? 0;
                this.RemoteFileName = response.Content?.Headers?.ContentDisposition?.FileName ?? String.Empty;
                //pr.FileSize = response.Content?.Headers?.ContentLength ?? 0; // this will change on resume
                this.BytesTransferred += read;
                this.Status = TaskStatus.Running;

                fs.Write(buffer, 0, read);
                fs.Flush();

                this.RunCalculations();

                read = inputStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}