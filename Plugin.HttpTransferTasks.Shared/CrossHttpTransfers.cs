using System;


namespace Plugin.HttpTransferTasks
{
    public static class CrossHttpTransfers
    {

        static IHttpTransferTasks current;
        public static IHttpTransferTasks Current
        {
            get
            {
#if BAIT
                if (current == null)
                    throw new ArgumentException("[Plugin.HttpTransferTasks] No platform plugin found.  Did you install the nuget package in your app project as well?");
#else
                current = current ?? new HttpTransferTasks();
#endif
                return current;
            }
            set { current = value; }
        }
    }
}
