using System.Diagnostics.CodeAnalysis;

namespace Application.Contracts
{
    public class CaseResult<T>
    {
        public T? Data { get; set; }
        [MemberNotNullWhen(true, nameof(Data))]
        public bool Successful { get; set; }
        /// <summary>User-facing message, already translated into the request language.</summary>
        public string? ErrorMessage { get; set; }
        public ErrorType ErrorType { get; set; } = ErrorType.Failure;

        public CaseResult(T data)
        {
            Data = data;
        }

        public CaseResult() { }

        public static CaseResult<T> Success(T data) => new(data) { Successful = true };

        public static CaseResult<T> Error(string message, ErrorType type = ErrorType.Failure) =>
            new() { Successful = false, ErrorMessage = message, ErrorType = type };
    }
}
