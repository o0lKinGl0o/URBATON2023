using System.Text;

namespace WebApplication3
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");

            // Логирование тела запроса
            context.Request.EnableBuffering();

            var body = context.Request.Body;
            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await body.ReadAsync(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            body.Position = 0;

            Console.WriteLine($"Request Body: {requestBody}");

            await _next(context);
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }

}
