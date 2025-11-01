using System.Text.Json.Serialization;

namespace Application
{
    public class GoogleUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        [JsonPropertyName("givenName")]
        public string FirstName { get; set; }
        [JsonPropertyName("familyName")]
        public string LastName { get; set; }
        [JsonPropertyName("photo")]
        public string PhotoAddress { get; set; }
    }
}
