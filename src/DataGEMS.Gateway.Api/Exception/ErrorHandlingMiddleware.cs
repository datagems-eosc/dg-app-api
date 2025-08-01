using Cite.Tools.Json;
using DataGEMS.Gateway.App.Exception;
using System.Net;

namespace DataGEMS.Gateway.Api.Exception
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly JsonHandlingService _jsonHandlingService;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            JsonHandlingService jsonHandlingService,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            this.next = next;
            _jsonHandlingService = jsonHandlingService;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (System.Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, System.Exception exception)
        {
            HandledException handled = HandleException(context, exception);

            _logger.Log(handled.Level, exception, "returning code {code} and payload {seralization}", handled.StatusCode, handled.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)handled.StatusCode;
            await context.Response.WriteAsync(handled.Message);
        }

        protected virtual HandledException HandleException(HttpContext context, System.Exception exception)
        {
            HttpStatusCode statusCode;
            object result;
            LogLevel logLevel;

            if (exception is DGNotFoundException)
            {
                logLevel = LogLevel.Debug;
                statusCode = HttpStatusCode.NotFound;

                int code = ((DGNotFoundException)exception).Code;
                if (code > 0) result = new { code, error = exception.Message };
                else result = new { error = exception.Message };
            }
            else if (exception is DGForbiddenException)
            {
                logLevel = LogLevel.Debug;
                statusCode = HttpStatusCode.Forbidden;

                int code = ((DGForbiddenException)exception).Code;
                if (code > 0) result = new { code, error = exception.Message };
                else result = new { error = exception.Message };
            }
			else if (exception is DGUnderpinningException)
			{
				logLevel = LogLevel.Error;
				statusCode = HttpStatusCode.FailedDependency;

                var payload = new
                {
                    statusCode = ((DGUnderpinningException)exception).ErrorStatusCode,
                    source = ((DGUnderpinningException)exception).ErrorSource?.ToString(),
                    correlationId = ((DGUnderpinningException)exception).CorrelationId,
                    payload = ((DGUnderpinningException)exception).ResponsePayload,
				};

				int code = ((DGUnderpinningException)exception).Code;
				if (code > 0) result = new { code, error = exception.Message, message = payload };
				else result = new { error = exception.Message, message = payload };
			}
			else if (exception is DGValidationException)
            {
                logLevel = LogLevel.Debug;
                statusCode = HttpStatusCode.BadRequest;

                int code = ((DGValidationException)exception).Code;
                if (code > 0) result = new { code, error = exception.Message, message = ((DGValidationException)exception).Errors };
                else result = new { error = exception.Message, message = ((DGValidationException)exception).Errors };
            }
            else if (exception is DGApplicationException)
            {
                logLevel = LogLevel.Error;
                statusCode = HttpStatusCode.InternalServerError;

                int code = ((DGApplicationException)exception).Code;
                if (code > 0) result = new { code, error = exception.Message };
                else result = new { error = exception.Message };
            }
            else
            {
                logLevel = LogLevel.Error;
                statusCode = HttpStatusCode.InternalServerError;

                string message = "System error";

                result = new { error = message };
            }

            string serialization = _jsonHandlingService.ToJsonSafe(result);

            return new HandledException() { StatusCode = statusCode, Message = serialization, Level = logLevel };
        }

        public class HandledException
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Message { get; set; }
            public LogLevel Level { get; set; }
        }
    }
}
