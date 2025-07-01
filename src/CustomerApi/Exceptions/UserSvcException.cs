using System.Net;

namespace CustomerCustomerApi.Exceptions;

public class UserSvcException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public UserSvcException()
        : base() { }

    public UserSvcException(string message) : base(message) { }

    public UserSvcException(string message, HttpStatusCode httpStatusCode) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public UserSvcException(ILogger logger, string message)
        : base(message)
    {
        logger.LogError(message);
    }

    public UserSvcException(string message, Exception exception)
        : base(message, exception)
    {
    }
    public UserSvcException(ILogger logger, string message, Exception exception)
    : base(message, exception)
    {
        logger.LogError(exception, message);
    }
}
