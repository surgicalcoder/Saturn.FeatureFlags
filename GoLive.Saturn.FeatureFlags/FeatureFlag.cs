using System;
using GoLive.Saturn.Data.Entities;

namespace GoLive.Saturn.FeatureFlags
{
    public class FeatureFlag : Entity
    {
        public String Method { get; set; }
        public DateTime LastRun { get; set; }
        public bool Run { get; set; }
        public int Interval { get; set; }

        public bool CanRun()
        {
            if (!Run)
            {
                return false;
            }

            var dateTime = LastRun.AddSeconds(Interval);
            return DateTime.Now > dateTime && Run;
        }
    }
}
