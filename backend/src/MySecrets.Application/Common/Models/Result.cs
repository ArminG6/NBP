namespace MySecrets.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(List<string> errors) => new(false, errors.FirstOrDefault(), errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? error, List<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public new static Result<T> Failure(string error) => new(false, default, error);
    public new static Result<T> Failure(List<string> errors) => new(false, default, errors.FirstOrDefault(), errors);
}
