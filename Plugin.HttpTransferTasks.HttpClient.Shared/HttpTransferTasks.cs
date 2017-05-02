using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Plugin.HttpTransferTasks.Models;
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.OS;
#endif


namespace Plugin.HttpTransferTasks
{
    public class HttpTransferTasks : AbstractHttpTransferTasks
    {
        readonly SqliteConnection conn;
#if __ANDROID__
        PowerManager.WakeLock wakeLock;
#endif

        public HttpTransferTasks()
        {
            this.conn = new SqliteConnection(); // this will block for a moment
#if __ANDROID__
            this.CurrentTasksChanged += (sender, args) =>
            {
                var powerManager = (PowerManager)Application.Context.GetSystemService(Context.PowerService);

                if (this.CurrentTasks.Count == 0)
                {
                    this.wakeLock?.Release();
                    this.wakeLock = null;
                }
                else if (this.wakeLock == null)
                {
                    this.wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "HTTPTRANSFERS");
                    this.wakeLock.Acquire();
                }
            };
#endif
            Task.Run(() =>
            {
                var cfgs = this.conn.TaskConfigurations.ToList();
                var headers = this.conn.TaskConfigurationHeaders.ToList();

                foreach (var cfg in cfgs)
                {
                    var cheaders = headers.Where(x => x.SqlTaskConfigurationId == cfg.Id).ToList();
                    this.RebuildTask(cfg, cheaders);
                }
            });
        }


        public override IHttpTask Upload(TaskConfiguration config)
        {
            if (String.IsNullOrWhiteSpace(config.LocalFilePath))
                throw new ArgumentException("You must set the file you wish to upload");

            if (!File.Exists(config.LocalFilePath))
                throw new ArgumentException("File to upload does not exist");

            return this.CreateTaskAndPersist(config, true);
        }


        public override IHttpTask Download(TaskConfiguration config)
        {
            if (String.IsNullOrWhiteSpace(config.LocalFilePath))
                config.LocalFilePath = Path.GetTempFileName();

            return this.CreateTaskAndPersist(config, false);
        }


        void OnTaskStatusChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(IHttpTask.Status))
                return;

            var task = (IHttpTask) sender;
            switch (task.Status)
            {
                case TaskStatus.Running:
                case TaskStatus.Resumed:
                case TaskStatus.Retrying:
                    break;

                default:
                    // base class will have removed from collection
                    task.PropertyChanged -= this.OnTaskStatusChanged;
                    var id = Int32.Parse(task.Identifier);
                    this.conn.Execute("DELETE FROM SqlTaskConfigurationHeader WHERE Id = ?", id);
                    this.conn.Execute("DELETE FROM SqlTaskConfiguration WHERE Id = ?", id);
                    break;
            }
        }


        IHttpTask RebuildTask(SqlTaskConfiguration sqlTask, IList<SqlTaskConfigurationHeader> headers)
        {
            var config = new TaskConfiguration(sqlTask.Uri, sqlTask.LocalFilePath)
            {
                HttpMethod = sqlTask.HttpMethod,
                PostData = sqlTask.PostData,
                UseMeteredConnection = sqlTask.UseMeteredConnection
            };

            foreach (var header in headers)
                config.Headers.Add(header.Key, header.Value);

            var task = this.CreateTask(config, sqlTask.IsUpload, sqlTask.Id.ToString());
            return task;
        }


        IHttpTask CreateTaskAndPersist(TaskConfiguration config, bool upload)
        {
            try
            {
                this.conn.BeginTransaction();

                var sqlTask = new SqlTaskConfiguration
                {
                    HttpMethod = config.HttpMethod,
                    Uri = config.Uri,
                    PostData = config.PostData,
                    LocalFilePath = config.LocalFilePath,
                    UseMeteredConnection = config.UseMeteredConnection,
                    IsUpload = upload
                };
                this.conn.Insert(sqlTask);

                foreach (var header in config.Headers)
                {
                    this.conn.Insert(new SqlTaskConfigurationHeader
                    {
                        SqlTaskConfigurationId = sqlTask.Id,
                        Key = header.Key,
                        Value = header.Value
                    });
                }
                var task = this.CreateTask(config, upload, sqlTask.Id.ToString());
                this.conn.Commit();

                return task;
            }
            catch
            {
                this.conn.Rollback();
                throw;
            }
        }


        IHttpTask CreateTask(TaskConfiguration config, bool upload, string identifier)
        {
            var task = new HttpTask(config, upload, identifier);
            task.PropertyChanged += this.OnTaskStatusChanged;
            this.Add(task);
            this.conn.Commit();

            return task;
        }
    }
}