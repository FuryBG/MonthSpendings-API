using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class CategorizeNotificationTransactionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
        [JsonPropertyName("createRule")]
        public bool CreateRule { get; set; }
    }
}
