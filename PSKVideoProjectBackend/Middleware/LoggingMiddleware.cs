namespace PSKVideoProjectBackend.Middleware
{
    public class LoggingMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Used to log all called requests upon start and finish
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="next"></param>
        public LoggingMiddleware(ILogger<LoggingMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        /// <summary>
        /// Logging executed by middleware
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var user = "Anonymous";

            //User.Identity.Name currently holds user Id
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                user = context.User.Identity.Name;

            _logger.LogInformation("Request '{Method}' in '{Path}' started by user '{User}' at {DateTime}", context.Request.Method, context.Request.Path, user, DateTime.Now);

            await _next(context);

            _logger.LogInformation("Request '{Method}' in '{Path}' finished with status code '{StatusCode}' by user '{User}' at {DateTime}", context.Request.Method, context.Request.Path, context.Response.StatusCode, user, DateTime.Now);
        }
    }
}
