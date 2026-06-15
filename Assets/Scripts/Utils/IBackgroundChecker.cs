using System;

namespace Utils
{
    public interface IBackgroundChecker
    {
        public void WhenBackFromBackground(TimeSpan time, DateTime now);
    }
}