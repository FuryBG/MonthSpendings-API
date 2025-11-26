using System.Diagnostics.CodeAnalysis;

namespace Application.Contracts
{
    public class CaseResult<T>
    {
        public T? Data { get; set; }
        [MemberNotNullWhen(true, nameof(Data))]
        public bool Successful { get; set; }
        public string? ErrorMessage { get; set; }

        public CaseResult(T data)
        {
            Data = data;
        }

        public CaseResult()
        {
            
        }
    }
}
