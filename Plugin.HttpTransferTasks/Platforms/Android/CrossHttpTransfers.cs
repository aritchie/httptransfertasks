using System;


namespace Plugin.HttpTransferTasks
{
    public static partial class CrossHttpTransfers
    {
        static CrossHttpTransfers()
        {
            Current = new HttpTransferTasks();
        }
    }
}
