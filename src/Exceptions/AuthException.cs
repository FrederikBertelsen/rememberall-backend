using RememberAll.src.Exceptions.Interfaces;

namespace RememberAll.src.Exceptions;

public class AuthException(string message, string internalMessage) : Exception(message.ToLower()), ICustomHttpException
{
    public string InternalMessage { get; init; } = internalMessage;

    public int StatusCode => 401;
}