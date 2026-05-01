namespace Application.Dto.SaltEdge
{
    public class SaltEdgeConnectionDto
    {
        public int Id { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? ValidTo { get; set; }
        public List<SaltEdgeAccountDto> Accounts { get; set; } = new();
    }
}
