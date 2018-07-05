using System;
using System.IO;
using Plugin.HttpTransferTasks.Models;
using SQLite;

namespace Plugin.HttpTransferTasks
{
    public class SqliteConnection : SQLiteConnection
    {
        public SqliteConnection() : base(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "pluginhttptransfer"), true)
        {
            this.CreateTable<SqlTaskConfiguration>();
            this.CreateTable<SqlTaskConfigurationHeader>();
        }


        public TableQuery<SqlTaskConfiguration> TaskConfigurations => this.Table<SqlTaskConfiguration>();
        public TableQuery<SqlTaskConfigurationHeader> TaskConfigurationHeaders => this.Table<SqlTaskConfigurationHeader>();
    }
}