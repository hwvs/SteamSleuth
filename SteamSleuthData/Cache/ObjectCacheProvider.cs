using System.Collections.Concurrent;

namespace SteamSleuthData.Cache;

public class CacheBucketKey
{
    public static readonly CacheBucketKey GetAppList_ListSteamAppModel = new CacheBucketKey("GetAppList_ListSteamAppModel");
    public static readonly CacheBucketKey AllAppIDs_ListInt = new CacheBucketKey("AllAppIDs_ListInt");

    private CacheBucketKey(string key)
    {
        this.Key = key;
    }

    private readonly string Key;

    //implicit
    public static implicit operator string(CacheBucketKey key)
    {
        return key.Key;
    }
}




public class ObjectCacheProvider
{
    private const int HRs = 60 * 60;
    private const int __STALE_MAX_CACHE_SECONDS = 4*HRs; // 4h is the max cache time
    
    private const int __DEFAULT_TTL_SECONDS = 30;
    private static ConcurrentDictionary<string, CacheBucket> Cache { get; set; } = new();
    
    private static DateTime LastCleaned { get; set; } = DateTime.MinValue;
    #if DEBUG
        private readonly static TimeSpan __CLEANUP_DELAY_BEFORE_NEXT = TimeSpan.FromMinutes(0.5);
        private readonly static TimeSpan __AUTO_CLEANUP_INTERVAL = TimeSpan.FromMinutes(1);
        private readonly static int __MAX_CACHE_SIZE = 100; // Max number of items in the cache
        private readonly static float __MAX_CACHE_SIZE_MAX_KEEP_RATIO = 0.5f; // If we're over the max cache size, keep this ratio of the cache
#else
        private readonly static TimeSpan __CLEANUP_DELAY_BEFORE_NEXT = TimeSpan.FromMinutes(1); // Need to wait at least 1 minute before running again
        private readonly static TimeSpan __AUTO_CLEANUP_INTERVAL = TimeSpan.FromMinutes(10); // Clean up the cache every 10 minutes
        private readonly static int __MAX_CACHE_SIZE = 10000; // Max number of items in the cache
        private readonly static float __MAX_CACHE_SIZE_MAX_KEEP_RATIO = 0.5f; // If we're over the max cache size, keep this ratio of the cache
    #endif
    private static object __cleanupMonitor = new object(); // Monitor to keep cache cleanup from running multiple times at once
    

    static ObjectCacheProvider()
    {
        
        // Clean up the cache every X minutes, with a Timer
        System.Timers.Timer timer = new System.Timers.Timer(__AUTO_CLEANUP_INTERVAL.TotalMilliseconds);
        timer.Elapsed += (sender, e) => CleanUpCache();
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Start();

    }
 
    internal static void CleanUpCache()
    {
        try
        {
            if(Monitor.TryEnter(__cleanupMonitor, TimeSpan.FromSeconds(1)) == false)
                return; // Already running, don't worry about it

            if (Cache.Count > __MAX_CACHE_SIZE)
            {
                Console.WriteLine("[CACHE] Cache is too big, clearing it out...");
            }
            else // We only check if it's too soon if the cache isn't too big, if it's too big then we have to clear it out.
            {
                if ((DateTime.Now - LastCleaned).TotalSeconds < __AUTO_CLEANUP_INTERVAL.TotalSeconds)
                    return; // Don't run it too often
            }


            LastCleaned = DateTime.Now;

            List<object> cleared = new List<object>();
            int count = 0;
            Console.WriteLine($"[CACHE] Cleaning up the cache... (Current Size: {Cache.Count} items)");

            List<KeyValuePair<string,CacheBucket>> ToClear = new List<KeyValuePair<string, CacheBucket>>();

            foreach (var bucket in Cache)
                ToClear.Add(bucket);
            
            if (Cache.Count > __MAX_CACHE_SIZE - 10) // Let's start clearing out the cache if it's getting too big
            {
                ToClear = ToClear.OrderByDescending(x => x.Value.LastUpdated).OrderByDescending(x => x.Value.IsPointerValid() ? -1 : x.Value.GetCacheHits())
                    .ToList();
                ToClear = ToClear.Take((int)(__MAX_CACHE_SIZE * __MAX_CACHE_SIZE_MAX_KEEP_RATIO)).ToList();
            }

            // Parallel foreach on the cache
            Parallel.ForEach(ToClear, (bucket) =>
            {
                bool shouldRemove = false;

                // Extremely old
                if (bucket.Value.IsExpired(__STALE_MAX_CACHE_SECONDS))
                    shouldRemove = true;

                // Reference is null or dead
                if (bucket.Value.IsPointerValid() == false)
                    shouldRemove = true;

                if (shouldRemove)
                {
                    bucket.Value.ClearCachedObject();
                    if (Cache.TryRemove(bucket.Key, out _))
                    {
                        count++;
                        if (count <= 5)
                        {
                            lock (cleared)
                            {
                                cleared.Add(bucket.Key);
                            }
                        }
                    }
                }
            });

            string itemNames = "N/A";
            if (cleared.Count > 0)
            {
                itemNames = String.Join(", ", cleared.ToArray());
                if (cleared.Count < count)
                    itemNames += $", [...] (Total Items: {count})";
            }

            Console.WriteLine(
                $"[CACHE] Cleaned up {count} items from the cache. Current Size: {Cache.Count} items. Removed: {itemNames}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CACHE] Error cleaning up the cache: {ex}");
        }
        finally
        {
            try
            {
                Monitor.Exit(__cleanupMonitor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CACHE] Error releasing the cleanup monitor: {ex.Message}");
            }
        }
    }
    
    public static bool SetBucket(CacheBucketKey key, object value, bool allowGarbageCollection = true, int ttlSeconds = -1)
    {
        CacheBucket bucket;
        if (!Cache.ContainsKey(key))
        {
            if (Cache.Count > __MAX_CACHE_SIZE)
            {
                // run this in a separate thread so it doesn't block the main thread
                System.Threading.ThreadPool.QueueUserWorkItem(delegate {
                    CleanUpCache();
                }, null);
            }
            
            bucket = new CacheBucket(__DEFAULT_TTL_SECONDS);
            if (false == Cache.TryAdd(key, bucket))
                return false; // Failure, don't worry about it, it's just a cache
        }
        else
        {
            if (false == Cache.TryGetValue(key, out bucket))
                return false; // Failure, don't worry about it, it's just a cache
        }

        bucket.SetCachedObject(value, allowGarbageCollection, ttlSeconds);
        return true;
    }
    
    public static T? GetBucket<T>(CacheBucketKey key, int ttlSeconds = -1)
    {
        if (Cache.ContainsKey(key))
        {
            CacheBucket bucket;
            if (false == Cache.TryGetValue(key, out bucket))
                return default(T?); // Failure, don't worry about it, it's just a cache
            
            if (bucket.IsExpired(ttlSeconds)) // If ttlSeconds is <0 then it will grab whatever was set on the object
            {
                bucket.ClearCachedObject(); // Automatically clear it when it's expired
                return default(T?);
            }
            
            return (T?)bucket.GetCachedObject();
        }
        
        return default(T?);
    }
    
    
}