using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VoyagesAPIService.Infrastructure.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<Startup> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(context, exception);

            }
        }

        private Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception.StackTrace);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400;

            if (context.Response.Headers["Status"].Count > 0)
                context.Response.Headers["Status"] = HttpStatusCode.ExpectationFailed.ToString();
            else
                context.Response.Headers.Add("Status", HttpStatusCode.ExpectationFailed.ToString());

            if (context.Response.Headers["Message"].Count > 0)
                context.Response.Headers["Message"] = exception.Message;
            else
                context.Response.Headers.Add("Message", exception.Message);

            Serilog.Log.Error(exception.StackTrace, "Error");
            return context.Response.WriteAsync(string.Empty);
        }
    }
}