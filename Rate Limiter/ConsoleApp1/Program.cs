
using System.Collections.Concurrent;

class RateLimiterConfig
{
    public double Capacity {  get; }
    public double RefillRate { get;  }
    public RateLimiterConfig(double capacity, double refillRate)
    {
        Capacity = capacity;    
        RefillRate = refillRate;
    }
}
class BucketState
{
    public double CurrentTokens;
    public DateTime LastRefillTime;
    public readonly object LockObj = new object();
    public BucketState(double initialTokens)
    {
        CurrentTokens = initialTokens;
        LastRefillTime = DateTime.UtcNow;
    }
}
interface IRateLimiterStrategy
{
    bool ShouldAllow(BucketState bucket, RateLimiterConfig config);
}
class TokenBucketStrategy : IRateLimiterStrategy
{
    public bool ShouldAllow(BucketState bucket, RateLimiterConfig config)
    {
        var now = DateTime.UtcNow;
        var secondElapsed = (now - bucket.LastRefillTime).TotalSeconds;
        bucket.CurrentTokens = Math.Min( config.Capacity , bucket.CurrentTokens + (config.RefillRate * secondElapsed)  );

        bucket.LastRefillTime = DateTime.UtcNow;
        if( bucket.CurrentTokens >=1 )
        {
            bucket.CurrentTokens -= 1;
            return true;
        }
        return false;
    }
}

class RateLimiter
{
    private readonly RateLimiterConfig config;
    private readonly IRateLimiterStrategy strategy;
    private readonly ConcurrentDictionary<string, BucketState> buckets = new ConcurrentDictionary<string, BucketState>();

    public RateLimiter(RateLimiterConfig config, IRateLimiterStrategy strategy)
    {
        this.config = config;
        this.strategy = strategy;
    }
    public bool Allow(string api_key)
    {
        var bucket = buckets.GetOrAdd(api_key, _ => new BucketState(config.Capacity));
        lock(bucket.LockObj)
        {
            return strategy.ShouldAllow(bucket, config);
        }
    }
}

class Program
{
    static void Main()
    {
        var config = new RateLimiterConfig(5, 1);
        var strategy = new TokenBucketStrategy();
        var rateLimiter = new RateLimiter(config, strategy);

        string api_key = "ABC123";
        Console.WriteLine("---- sending 7 requested back to back ----");
        for(int i = 0;i <7; i++)
        {
            bool allowed = rateLimiter.Allow(api_key);
            Console.WriteLine($"Request {i}: {(allowed ? "✅ ALLOWED" : "❌ REJECTED")}");
        }
        Console.WriteLine("\n=== Waiting 3 seconds (tokens refill) ===\n");
        System.Threading.Thread.Sleep(3000);
        for (int i = 8; i <= 11; i++)
        {
            bool allowed = rateLimiter.Allow(api_key);
            Console.WriteLine($"Request {i}: {(allowed ? "✅ ALLOWED" : "❌ REJECTED")}");
        }
    }
}