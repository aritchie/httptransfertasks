using System;
using SQLite;


namespace Plugin.HttpTransferTasks.Models
{
    public class SqlTaskConfigurationHeader
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int SqlTaskConfigurationId { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}