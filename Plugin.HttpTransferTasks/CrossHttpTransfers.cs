using System;


namespace Plugin.HttpTransferTasks
{
    public static partial class CrossHttpTransfers
    {
        static IHttpTransferTasks current;
        public static IHttpTransferTasks Current
        {
            get
            {
                if (current == null)
                    throw new ArgumentException("[Plugin.HttpTransferTasks] No platform plugin found.  Did you install the nuget package in your app project as well?");

                return current;
            }
            set => current = value;
        }
    }
}
