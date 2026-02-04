using Lesson_3.Models.Interfases;

namespace Lesson_3.Middleware
{
    public class ExchangeMiddleware
    {
        private readonly RequestDelegate _next;

        public ExchangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/exchangeRate"))
            {
                await _next(context);
                return;
            }

            var rates = context.Items["Rates"] as List<IDto>;
            if (rates == null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return;
            }
            var parts = context.Request.Path.Value?.Split("/");
            if (parts == null || parts.Length < 3)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            var code = parts[2].ToUpper();
            var currency = rates.FirstOrDefault(r => r.CurrencyCode == code);
            if (currency == null)
            {
                await context.Response.WriteAsync("Currency not found");
                return;
            }
            await context.Response.WriteAsJsonAsync(currency);

        }


    }
}
