namespace SteamSleuthData.Cache;

internal class CacheBucket
{
    private int TtlSeconds ;
    public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    
    private static bool isWeakReference;
    private WeakReference<object> WeakReferenceCachedObject = null;
    private object StrongReferenceCachedObject = null;
    
    private int CacheHits = 0; // Keep track of how many times we've hit this cache bucket

    public CacheBucket(int ttlSeconds)
    {
        TtlSeconds = ttlSeconds;
    }

    public int GetCacheHits()
    {
        return CacheHits;
    }
    
    public void SetCachedObject(object cachedObject, bool allowGarbageCollection = true, int ttlSeconds = -1)
    {
        ClearCachedObject(); // Clear the old object
        
        // Weak Ref Mode
        isWeakReference = allowGarbageCollection;
        if (isWeakReference)
        {
            WeakReferenceCachedObject = new WeakReference<object>(cachedObject);
        }
        else
        {
            StrongReferenceCachedObject = cachedObject;
        }

        LastUpdated = DateTime.Now;
        
        if(ttlSeconds > 0)
            TtlSeconds = ttlSeconds;
    }
    
    public void ClearCachedObject()
    {
        CacheHits = 0;
        WeakReferenceCachedObject = null;
        StrongReferenceCachedObject = null;
        LastUpdated = DateTime.MinValue;
    }
    
    public bool IsExpired(int ttlSeconds = -1)
    {
        return LastUpdated.AddSeconds((ttlSeconds > 0 ? ttlSeconds : TtlSeconds)) < DateTime.Now;
    }
    
    public bool IsPointerValid()
    {
        if (isWeakReference)
        {
            if (WeakReferenceCachedObject != null)
            {
                if (WeakReferenceCachedObject.TryGetTarget(out object cachedObject))
                {
                    return true;
                }
            }
        }
        else
        {
            return StrongReferenceCachedObject != null;
        }

        return false;
    }
    
    public object? GetCachedObject()
    {
        if (isWeakReference)
        {
           if (WeakReferenceCachedObject != null)
           {
               if (WeakReferenceCachedObject.TryGetTarget(out object cachedObject))
               {
                   CacheHits++;
                   return cachedObject; // If null then it returns null, so that's no issue
               }
           } 
        }
        else
        {
            CacheHits++;
            return StrongReferenceCachedObject;
        }

        return null;
    }

}