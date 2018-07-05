using System;
using System.IO;
using Acr.IO;
using SQLite;


namespace Plugin.HttpTransferTasks
{
    public class SqliteConnection : SQLiteConnection
    {
        public SqliteConnection() : base(Path.Combine(FileSystem.Current.AppData.FullName, "pluginhttp.db"), true)
        {
            this.CreateTable<SqlTaskConfiguration>();
            this.CreateTable<SqlTaskConfigurationHeader>();
        }


        public TableQuery<SqlTaskConfiguration> TaskConfigurations => this.Table<SqlTaskConfiguration>();
        public TableQuery<SqlTaskConfigurationHeader> TaskConfigurationHeaders => this.Table<SqlTaskConfigurationHeader>();
    }
}