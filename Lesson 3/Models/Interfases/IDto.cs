namespace Lesson_3.Models.Interfases
{
    public interface IDto
    {
        string CurrencyCode { get; }
        decimal Rate { get; }
    }
}