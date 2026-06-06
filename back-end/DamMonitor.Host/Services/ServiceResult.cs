namespace DamMonitor.Host.Services;

public enum ServiceResultStatus
{
    Success,
    NotFound,
    Conflict,
    BadRequest
}

public sealed record ServiceResult<T>(ServiceResultStatus Status, T? Value, string? Error)
{
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value, null);
    public static ServiceResult<T> NotFound(string error) => new(ServiceResultStatus.NotFound, default, error);
    public static ServiceResult<T> Conflict(string error) => new(ServiceResultStatus.Conflict, default, error);
    public static ServiceResult<T> BadRequest(string error) => new(ServiceResultStatus.BadRequest, default, error);
}
