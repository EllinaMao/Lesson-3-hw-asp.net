using Lesson_3.Middleware;
using Lesson_3.Models;
using Lesson_3.Models.Interfases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();//to get json from Api (hell no, i will not use monobank api again >:c)
builder.Services.AddScoped<ICurrencyService, NbuCurrencyService>();
var app = builder.Build();

app.UseMiddleware<ErrorCodesMiddleware>();
app.UseMiddleware<CurrencyMiddleware>();
app.UseMiddleware<ConvertMiddleware>();
app.UseMiddleware<ExchangeMiddleware>();

app.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await Task.CompletedTask;
});

app.Run();