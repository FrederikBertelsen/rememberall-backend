using RememberAll.src.Exceptions.Interfaces;

namespace RememberAll.src.Exceptions;

public class BusinessLogicException(string message) : Exception(message.ToLower()), ICustomHttpException
{
    public int StatusCode => 400;
}