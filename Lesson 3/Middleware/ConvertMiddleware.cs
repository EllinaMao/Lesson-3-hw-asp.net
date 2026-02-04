using Lesson_3.Models.Interfases;

namespace Lesson_3.Middleware
{
    public class ConvertMiddleware
    {
        private readonly RequestDelegate _next;
        public ConvertMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/convertCurrency"))
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
            var parts = context.Request.Path.Value?.Split('/');
            if (parts == null || parts.Length < 5)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            string from = parts[2].ToUpper();
            string to = parts[3].ToUpper();

            if (!decimal.TryParse(parts[4], out decimal amount))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            decimal rateFrom = from == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == from)?.Rate ?? 0;
            decimal rateTo = to == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == to)?.Rate ?? 0;
            if (rateFrom == 0 || rateTo == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            decimal result = (amount * rateFrom) / rateTo;

            await context.Response.WriteAsJsonAsync(new
            {
                From = from,
                To = to,
                SourceAmount = amount,
                Result = Math.Round(result, 2)
            });
        }
    }
}
