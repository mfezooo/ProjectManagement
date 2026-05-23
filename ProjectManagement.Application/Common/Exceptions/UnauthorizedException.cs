namespace ProjectManagement.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Unauthorized.") { }
    public UnauthorizedException(string message) : base(message) { }
}
