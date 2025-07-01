using System.Net;

namespace CustomerCustomerApi.Exceptions;

public class GraphServiceException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public GraphServiceException()
        : base() { }

    public GraphServiceException(string message) : base(message) { }

    public GraphServiceException(ILogger logger, string message)
        : base(message)
    {
        logger.LogError(message);
    }

    public GraphServiceException(string message, Exception exception)
        : base(message, exception)
    {
    }
    public GraphServiceException(ILogger logger, string message, Exception exception)
    : base(message, exception)
    {
        logger.LogError(exception, message);
    }
}
