using System;

namespace GoLive.Saturn.FeatureFlags
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class FeatureFlagAttribute : Attribute
    {
        public FeatureFlagAttribute(bool run = true, int interval = 1800)
        {
            Run = run;

            Interval = interval;
        }

        public bool Run { get; set; }
        public int Interval { get; set; }
    }
}