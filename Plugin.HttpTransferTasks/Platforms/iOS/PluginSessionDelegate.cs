using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;

namespace Plugin.HttpTransferTasks
{
    public class PluginSessionDelegate : NSUrlSessionDownloadDelegate
    {
        readonly IDictionary<string, IIosHttpTask> tasks = new ConcurrentDictionary<string, IIosHttpTask>();
        readonly object syncLock = new object();


        public void AddTask(IIosHttpTask task)
        {
            lock (this.syncLock)
                this.tasks.Add(task.Identifier, task);
        }


        //public override void DidBecomeInvalid(NSUrlSession session, NSError error)
        //    Console.WriteLine("Session became invalid");

        public override void DidResume(NSUrlSession session, NSUrlSessionDownloadTask task, long resumeFileOffset, long expectedTotalBytes)
            => this.DoAction(task, item => item.SetResumeOffset(resumeFileOffset, expectedTotalBytes));


        public override void DidWriteData(NSUrlSession session,
                                          NSUrlSessionDownloadTask task,
                                          long bytesWritten,
                                          long totalBytesWritten,
                                          long totalBytesExpectedToWrite)
            => this.DoAction(task, item => item.SetData(
                bytesWritten,
                totalBytesWritten,
                totalBytesExpectedToWrite
            ));


        public override void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask task, NSUrl location) => this.DoAction(task, item =>
        {
            if (item.Exception == null && this.tasks.Remove(item.Identifier))
                item.SetDownloadComplete(location.Path);
        });


        //public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
        //{
        //    base.DidFinishEventsForBackgroundSession(session);
        //}


        public override void DidReceiveChallenge(NSUrlSession session, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
            => completionHandler.Invoke(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);


        public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
            => this.DoAction(task, item => {
                item.SetError(error);
                this.tasks.Remove(item.Identifier);
            });


        public override void DidSendBodyData(NSUrlSession session,
                                             NSUrlSessionTask task,
                                             long bytesSent,
                                             long totalBytesSent,
                                             long totalBytesExpectedToSend)
            // TODO: I believe this will fire for uploads and downloads - need to identify direction as I don't want to fire this for downloads ONLY uploads
            // TODO: what if upload is done?
            => this.DoAction(task, item => item.SetData(
                bytesSent,
                totalBytesSent,
                totalBytesExpectedToSend
            ));


        void DoAction(NSUrlSessionTask task, Action<IIosHttpTask> processAction)
        {
            lock (this.syncLock)
            {
                // TODO: resuming is not going well here
                var id = task.TaskIdentifier.ToString();
                if (this.tasks.ContainsKey(id))
                {
					var item = this.tasks[task.TaskIdentifier.ToString()];
					processAction(item);
                }
            }
        }
    }
}