using Lesson_3.Models.Interfases;
namespace Lesson_3.Middleware
{
    public class CurrencyMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrencyMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task InvokeAsync(HttpContext context, ICurrencyService currencyService)
        {

            try
            {
                var rates = await currencyService.GetRatesAsync();
                context.Items["Rates"] = rates;
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            await _next(context);
        }
    }
}