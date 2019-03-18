using System;
using System.Linq;
using System.Threading.Tasks;
using GoLive.Saturn.Data.Abstractions;

namespace GoLive.Saturn.FeatureFlags
{
    public static class Flags
    {
        public static IRepository repository;

        public static async Task<bool> Exists(string CacheKey)
        {
            string type = CacheKey;

            var item = await repository.One<FeatureFlag>(r => r.Method == type);

            if (item == null)
            {
                return false;
            }
            var needsToRun = item.LastRun.AddSeconds(item.Interval) < DateTime.Now;

            return !needsToRun;
        }

        public static async Task SaveRunning(string CacheKey, int interval = 1800, bool run = true)
        {
            string type = CacheKey;

            var item = (await repository.One<FeatureFlag>(r => r.Method == type)) ?? new FeatureFlag()
            {
                Method = CacheKey,
                Interval = interval,
                Run = run,
            };

            item.LastRun = DateTime.Now;
            await repository.Upsert(item);
        }

        public static async Task<bool> RunIfFlagged(string CacheKey)
        {
            string type = CacheKey;

            var item = await repository.One<FeatureFlag>(r => r.Method == type);

            if (item == null)
            {
                item = new FeatureFlag
                {
                    Method = CacheKey,
                    Interval = 1800,
                    Run = true,
                    LastRun = DateTime.UtcNow
                };
                await repository.Add(item);

                return true;
            }

            if (!item.CanRun())
            {
                return false;
            }

            var needsToRun = item.LastRun.AddSeconds(item.Interval) < DateTime.UtcNow;

            if (needsToRun)
            {
                item.LastRun = DateTime.UtcNow;
                await repository.Upsert(item);
            }

            return needsToRun;
        }


        public static async Task RunIfFlagged(Action action)
        {
            var actionMethod = action.Method;

            var firstOrDefault = actionMethod.GetCustomAttributes(typeof(FeatureFlagAttribute), true).FirstOrDefault();

            if (firstOrDefault == null)
            {
                action.Invoke();
                return;
            }

            if (!(firstOrDefault is FeatureFlagAttribute flag))
            {
                action.Invoke();
                return;
            }

            string type = actionMethod.DeclaringType.FullName + "." + actionMethod.Name;

            var item = await repository.One<FeatureFlag>(r => r.Method == type);

            if (item == null)
            {

                item = new FeatureFlag
                {
                    Interval = flag.Interval,
                    Run = flag.Run
                };
                await repository.Add(item);

                action.Invoke();
                return;
            }

            if (item.CanRun())
            {
                action.Invoke();
                return;
            }

        }
    }
}