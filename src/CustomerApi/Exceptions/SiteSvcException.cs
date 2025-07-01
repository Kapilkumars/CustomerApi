using System.Net;

namespace CustomerCustomerApi.Exceptions;

public class SiteSvcException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public SiteSvcException()
        : base() { }

    public SiteSvcException(string message) : base(message) { }

    public SiteSvcException(ILogger logger, string message)
        : base(message)
    {
        logger.LogError(message);
    }

    public SiteSvcException(string message, Exception exception)
        : base(message, exception)
    {
    }
    public SiteSvcException(ILogger logger, string message, Exception exception)
    : base(message, exception)
    {
        logger.LogError(exception, message);
    }
}
