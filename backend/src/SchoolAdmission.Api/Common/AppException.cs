namespace SchoolAdmission.Api.Common;

public class AppException : Exception
{
    public AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(StatusCodes.Status404NotFound, message)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(StatusCodes.Status400BadRequest, message)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(StatusCodes.Status401Unauthorized, message)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(StatusCodes.Status403Forbidden, message)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(StatusCodes.Status409Conflict, message)
    {
    }
}
