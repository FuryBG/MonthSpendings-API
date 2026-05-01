namespace Application.Dto.SaltEdge
{
    public class SaltEdgeConnectionStatusDto
    {
        public Guid LocalSessionId { get; set; }
        public string State { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
