namespace Aviation.WebApi.Errors;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    protected AppException(int statusCode, string code, string message) : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }
}

public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message, string code = "NOT_FOUND")
        : base(StatusCodes.Status404NotFound, code, message) { }
}

public sealed class BadRequestAppException : AppException
{
    public BadRequestAppException(string message, string code = "BAD_REQUEST")
        : base(StatusCodes.Status400BadRequest, code, message) { }
}

public sealed class ConflictAppException : AppException
{
    public ConflictAppException(string message, string code = "CONFLICT")
        : base(StatusCodes.Status409Conflict, code, message) { }
}
