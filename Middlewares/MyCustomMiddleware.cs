namespace Middlewares
{
    public class MyCustomMiddleware
    {
        private readonly RequestDelegate _next;

        public MyCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine("Before request");
            await _next(context); // Call the next middleware or controller
            Console.WriteLine("After request");
        }
    }

}
