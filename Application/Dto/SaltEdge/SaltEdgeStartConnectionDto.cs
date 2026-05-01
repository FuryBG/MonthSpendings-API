namespace Application.Dto.SaltEdge
{
    public class SaltEdgeStartConnectionDto
    {
        public string ConnectUrl { get; set; } = string.Empty;
        public Guid LocalSessionId { get; set; }
    }
}
