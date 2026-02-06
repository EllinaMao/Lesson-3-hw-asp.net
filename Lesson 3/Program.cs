
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
app.Map("/currencies", async (ICurrencyService service) =>
{
    var rates = await service.GetRatesAsync();
    return Results.Json(rates);
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
            await context.Response.WriteAsJsonAsync(new { error = "Usage: /exchangeRate/CURRENCY or /exchangeRate/FROM/TO" });
            return;
        }

        var service = context.RequestServices.GetRequiredService<ICurrencyService>();
        var rates = await service.GetRatesAsync();


        if (parts.Length == 1)
        {
            string currencyCode = parts[0].ToUpper();
            var currency = rates.FirstOrDefault(r => r.CurrencyCode == currencyCode);

            if (currency == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { error = "Currency not found" });
                return;
            }

            await context.Response.WriteAsJsonAsync(currency);
        }

        else if (parts.Length >= 2)
        {
            string from = parts[0].ToUpper();
            string to = parts[1].ToUpper();

            decimal rateFrom = from == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == from)?.Rate ?? 0;
            decimal rateTo = to == "UAH" ? 1 : rates.FirstOrDefault(r => r.CurrencyCode == to)?.Rate ?? 0;

            if (rateFrom == 0 || rateTo == 0)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { error = "Currency not found" });
                return;
            }

            decimal exchangeRate = rateFrom / rateTo;

            await context.Response.WriteAsJsonAsync(new
            {
                From = from,
                To = to,
                Rate = Math.Round(exchangeRate, 4)
            });
        }
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

app.Map("/", async (context) => {
    context.Response.ContentType = "text/html; charset=utf-8";
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
                <h1>Homework</h1>
                <p>Use the following endpoints:</p>
                <ul>
                    <li><strong>/currencies</strong> - Get list of all supported currencies</li>
                    <li><strong>/exchangeRate/CURRENCY</strong> - Get info for one currency (e.g., /exchangeRate/USD)</li>
                    <li><strong>/exchangeRate/FROM/TO</strong> - Get exchange rate between two currencies (e.g., /exchangeRate/USD/EUR)</li>
                    <li><strong>/convertCurrency/FROM/TO/AMOUNT</strong> - Convert specific amount (e.g., /convertCurrency/USD/EUR/100)</li>
                </ul>
            </div>
        </body>
        </html>";

    await context.Response.WriteAsync(htmlPage);
});

app.Run();