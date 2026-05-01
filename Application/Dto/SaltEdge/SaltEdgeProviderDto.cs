namespace Application.Dto.SaltEdge
{
    public class SaltEdgeProviderDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Mode { get; set; }
        public bool Regulated { get; set; }
    }
}
