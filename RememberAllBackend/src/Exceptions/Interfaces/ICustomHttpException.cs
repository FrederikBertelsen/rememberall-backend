namespace RememberAll.src.Exceptions.Interfaces;

public interface ICustomHttpException
{
    int StatusCode { get; }
}