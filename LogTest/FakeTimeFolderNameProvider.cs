using System;

namespace LogTest
{
    public class FakeTimeFolderNameProvider : IFolderNameProvider
    {
        private DateTime fakeTime;

        public FakeTimeFolderNameProvider(int hour, int minute)
        {
            var now = DateTime.UtcNow;
            fakeTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
        }

        public void AdvanceTimeByOneHour()
        {
            fakeTime = fakeTime.AddHours(1);
        }

        public string GetFolderName()
        {
            var datePath = fakeTime.ToString("yyyy-MM-dd");
            return datePath;
        }
    }
}
