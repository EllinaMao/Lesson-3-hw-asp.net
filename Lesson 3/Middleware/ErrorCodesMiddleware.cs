using System.Net;
using System.Text.Json;

namespace Lesson_3.Middleware
{
    public class ErrorCodesMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorCodesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await HandleErrorHtmlAsync(context);
                return;
            }

            if (context.Response.StatusCode >= 400)
            {
                await HandleErrorHtmlAsync(context);
            }
        }

        private async Task HandleErrorHtmlAsync(HttpContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            var code = context.Response.StatusCode;
            var message = GetFriendlyMessage(code);

            string htmlPage = $@"
            <!DOCTYPE html>
            <html lang='ru'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Ошибка {code}</title>    
                
            </head>
            <body>
                <div>
                    <h1>{code}</h1>
                    <h3 style = 'color:red'>Error {code}.</h3>
                    <p>{message}</p>
                    
                    <a href='/'>⬅ Вернуться на главную</a>
                </div>
            </body>
            </html>";

            await context.Response.WriteAsync(htmlPage);
        }

        private string GetFriendlyMessage(int code)
        {
            return code switch
            {
                400 => "Bad Request. Please check your URL parameters.",
                404 => "Resource or Currency not found.",
                500 => "Internal Server Error. We are fixing it!",
                503 => "Service Unavailable. Rates are not loaded yet.",
                _ => "An unexpected error occurred."
            };
        }
    }
}