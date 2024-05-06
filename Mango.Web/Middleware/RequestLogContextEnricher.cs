using Serilog.Context;

namespace Mango.Web.Middleware
{
    public class RequestLogContextEnricher
    {
        private readonly RequestDelegate _next;

        public RequestLogContextEnricher(RequestDelegate next)
        {  
            _next = next; 
        }

        public Task InvokeAsync(HttpContext context)
        {
            using(LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
            {
                return _next(context);
            }
        }
    }
}
