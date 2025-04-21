using System.Collections.Concurrent;

namespace Middlewares
{
    public class MyCustomMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs = new();
        //This stores:Key: the client’s IP address Value: list of times when that IP made requests 📦 Example {"192.168.1.5": [10:00:01, 10:00:03]}
        //string to store Client IP and List<DateTime> to store the request timestamps
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private const int Limit = 1;              // Max requests
        private readonly TimeSpan TimeWindow = TimeSpan.FromSeconds(3); // Per time window

        public MyCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine("Before request");
            
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var now = DateTime.UtcNow;
            var log = _requestLogs.GetOrAdd(clientIp, new List<DateTime>());
            var ipLock = _locks.GetOrAdd(clientIp, new SemaphoreSlim(1, 1));
            Console.WriteLine($"Client IP: {clientIp},  Time: {now}");
            await ipLock.WaitAsync(); // Async-safe locking

            try
            {
                // Clean up old timestamps
                log.RemoveAll(t => (now - t) > TimeWindow);

                if (log.Count >= Limit)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "text/plain";
                    context.Response.Headers["Retry-After"] = "10";
                    context.Response.Headers["X-Rate-Limit-Remaining"] = "0";
                    context.Response.Headers["X-Rate-Limit-Reset"] = (now + TimeWindow).ToString("o");

                    await context.Response.WriteAsync("🚫 Too many requests. Please wait and try again.");
                    return;
                }

                log.Add(now);
            }
            finally
            {
                ipLock.Release(); // Always release the lock
            }


            await _next(context); // Continue if not in maintenance
            Console.WriteLine("After request");
        }
    }
}
