using System;
using SQLite;


namespace Plugin.HttpTransferTasks.Models
{
    public class SqlTaskConfiguration
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public bool IsUpload { get; set; }

        public string Uri { get; set; }
        public string LocalFilePath { get; set; }
        public bool UseMeteredConnection { get; set; }
        public string HttpMethod { get; set; }
        public string PostData { get; set; }
    }
}