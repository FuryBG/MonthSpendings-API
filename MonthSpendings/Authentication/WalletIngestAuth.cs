namespace MonthSpendings.Authentication
{
    /// <summary>
    /// JWT scheme for the phone's background wallet listener. Its tokens carry the <c>Jwt:WalletAudience</c> audience,
    /// which the default scheme rejects, so they are only accepted where this scheme is named explicitly.
    /// </summary>
    public static class WalletIngestAuth
    {
        public const string Scheme = "WalletIngest";
    }
}
