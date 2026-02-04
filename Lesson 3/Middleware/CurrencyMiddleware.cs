using Lesson_3.Models.Interfases;
namespace Lesson_3.Middleware
{
    public class CurrencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public CurrencyMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, ICurrencyService currencyService)
        {
            //its annoing
            if (context.Request.Path.Value?.Contains("favicon") == true)
            {
                await _next(context);
                return;
            }

            try
            {
                var rates = await currencyService.GetRatesAsync();
                context.Items["Rates"] = rates;
            }
            catch (Exception ex)//i wanted to try from last lesson
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                if (_env.IsDevelopment())
                {
                    Console.WriteLine(ex.Message);
                    await context.Response.WriteAsync($"Service Error: {ex.Message}");
                }
                else
                {
                    await context.Response.WriteAsync("Currency service unavailable.");
                }
                return;
            }

            await _next(context);
        }
    }
}