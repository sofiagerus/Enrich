namespace Enrich.BLL.Common
{
    public class Result
    {
        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        protected Result(bool isSuccess, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new(true, null);

        public static Result Failure(string errorMessage) => new(false, errorMessage);

        public static implicit operator Result(string errorMessage)
        {
            return Failure(errorMessage);
        }

        public static implicit operator Result(bool success)
        {
            return success ? Success() : Failure("Operation failed.");
        }
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool isSuccess, T? value, string? errorMessage)
            : base(isSuccess, errorMessage)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, null);

        public static new Result<T> Failure(string errorMessage) => new(false, default, errorMessage);

        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }

        public static implicit operator Result<T>(string errorMessage)
        {
            return Failure(errorMessage);
        }
    }
}
