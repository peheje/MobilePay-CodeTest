using System;

namespace LogTest
{
    public class UtcTimeFolderNameProvider : IFolderNameProvider
    {
        public string GetFolderName()
        {
            var now = DateTime.UtcNow;
            var datePath = now.ToString("yyyy-MM-dd");
            return datePath;
        }
    }
}
