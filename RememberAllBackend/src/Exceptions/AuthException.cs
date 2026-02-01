using RememberAll.src.Exceptions.Interfaces;

namespace RememberAll.src.Exceptions;

public class AuthException(string internalMessage) : Exception("Authentication failed"), ICustomHttpException
{
    public string InternalMessage { get; init; } = internalMessage;

    public int StatusCode => 401;
}