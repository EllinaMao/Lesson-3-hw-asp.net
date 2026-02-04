using System.Text.Json.Serialization;
using Lesson_3.Models.Interfases;

namespace Lesson_3.Models
{
    public class NbuRate:IDto
    {
        [JsonPropertyName("cc")]     
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("rate")]   
        public decimal Rate { get; set; }

        [JsonPropertyName("txt")]   
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("exchangedate")]
        public string Date { get; set; } = string.Empty;
    }
}