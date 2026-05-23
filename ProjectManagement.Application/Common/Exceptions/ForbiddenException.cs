namespace ProjectManagement.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You are not allowed to perform this action.") { }
    public ForbiddenException(string message) : base(message) { }
}
