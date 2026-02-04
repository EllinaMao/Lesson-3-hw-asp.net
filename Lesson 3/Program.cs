
/*перечитала задание, подумала и пошла делать как сказано. Код на компонентах оставила в папке oldCode*/
using Lesson_3.Models;
using Lesson_3.Models.Interfases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<ICurrencyService, NbuCurrencyService>();//hell no i will not use monobank api again...

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
    }
});

app.Map("/exchangeRate", exchangeApp =>
{
    exchangeApp.Run(async context =>
    {
        var path = context.Request.Path.Value;
        var parts = path?.Split('/', StringSplitOptions.RemoveEmptyEntries); 

        if (parts == null || parts.Length < 1)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid currency parameters" });
            return;
        }

        string currencyCode = parts[0].ToUpper();


        var service = context.RequestServices.GetRequiredService<ICurrencyService>();
        var rates = await service.GetRatesAsync();

        var currency = rates.FirstOrDefault(r => r.CurrencyCode == currencyCode);

        if (currency == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Currency not found" });
            return;
        }

        await context.Response.WriteAsJsonAsync(currency);
    });
});

app.Map("/convertCurrency", convertApp =>
{
    convertApp.Run(async context =>
    {

        var path = context.Request.Path.Value;
        var parts = path?.Split('/', StringSplitOptions.RemoveEmptyEntries); // ["USD", "EUR", "100"]

        if (parts == null || parts.Length < 3)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Usage: /convertCurrency/FROM/TO/AMOUNT" });
            return;
        }

        string from = parts[0].ToUpper();
        string to = parts[1].ToUpper();

        if (!decimal.TryParse(parts[2], out decimal amount))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid amount" });
            return;
        }

        var service = context.RequestServices.GetRequiredService<ICurrencyService>();
        var rates = await service.GetRatesAsync();

        // bc NBU rates are against UAH and i wanted to use api
        decimal rateFrom = from == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == from)?.Rate ?? 0;
        decimal rateTo = to == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == to)?.Rate ?? 0;

        if (rateFrom == 0 || rateTo == 0)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Currency not found" });
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
    });
});

app.Map("/", async (context) =>
{
    context.Response.ContentType = "text/html";
    string htmlPage = @"
        <!DOCTYPE html>
        <html lang='ru'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Currency Exchange API</title>    
        </head>
        <body>
            <div>
                <h1>Welcome to the Currency Exchange API</h1>
                <p>Use the following endpoints:</p>
                <ul>
                    <li>/exchangeRate/CURRENCY_CODE - Get exchange rate for a specific currency (e.g., /exchangeRate/USD/EUR)</li>
                    <li>/convertCurrency/FROM/TO/AMOUNT - Convert amount from one currency to another (e.g., /convertCurrency/USD/EUR/100)</li>
                </ul>
            </div>
        </body>
        </html>";


});

app.Run(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsJsonAsync(new { error = "Page not found" });
});

app.Run();