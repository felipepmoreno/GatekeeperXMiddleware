using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;

namespace GatekeeperX
{
    public class GatekeeperXValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public GatekeeperXValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body) && !ValidateJsonInput(body))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid input data.");
                        return;
                    }
                }
                context.Request.Body.Position = 0;
            }
            await _next(context);
        }

        private bool ValidateJsonInput(string json)
        {
            return true;
        }
    }
}
