using System;

namespace Utils
{
    public interface IDayDiffChecker
    {
        public void HandleDayDiff(DateTime now, int dayDiff);
    }
}