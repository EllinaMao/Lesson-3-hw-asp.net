namespace Lesson_3.Models.Interfases
{
    public interface ICurrencyService
    {
        Task<List<IDto>> GetRatesAsync();
    }
}
