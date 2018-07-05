using System;
using System.IO;
using Plugin.HttpTransferTasks.Models;
using SQLite;

namespace Plugin.HttpTransferTasks
{
    public class SqliteConnection : SQLiteConnection
    {
        public SqliteConnection() : base(Path.Combine(FileSystem.Current.AppDataDirectory.FullName, "pluginhttp.db"), true)
        {
            this.CreateTable<SqlTaskConfiguration>();
            this.CreateTable<SqlTaskConfigurationHeader>();
        }


        public TableQuery<SqlTaskConfiguration> TaskConfigurations => this.Table<SqlTaskConfiguration>();
        public TableQuery<SqlTaskConfigurationHeader> TaskConfigurationHeaders => this.Table<SqlTaskConfigurationHeader>();
    }
}