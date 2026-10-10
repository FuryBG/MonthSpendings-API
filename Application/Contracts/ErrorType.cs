namespace Application.Contracts
{
    /// <summary>Kind of failure; the value is the HTTP status the API responds with.</summary>
    public enum ErrorType
    {
        Failure = 400,
        Unauthorized = 401,
        ProRequired = 402,
        Conflict = 409,
        Locked = 423
    }
}
