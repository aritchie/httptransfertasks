using System;
using System.Diagnostics;


namespace Plugin.HttpTransferTasks
{
    public static class Logger
    {
        public static Action<string> Out { get; set; } = s => Debug.WriteLine(s);
        public static void WriteLine(string msg) => Out?.Invoke(msg);
    }
}
