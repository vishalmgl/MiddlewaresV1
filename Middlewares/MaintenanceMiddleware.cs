namespace Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _isInMaintenance = true;
        public MaintenanceMiddleware(RequestDelegate next) 
        { 
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if(_isInMaintenance)
            {
                context.Response.StatusCode = 503;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("🚧 The site is under maintenance. Please try again later.");
                return;
            }
            await _next(context); // Call the next middleware or controller
        }
    }
}
