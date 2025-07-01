using System.Net;

namespace CustomerCustomerApi.Exceptions
{
    public class ServiceException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public ServiceException()
            : base() { }

        public ServiceException(string message) : base(message) { }

        public ServiceException(string message, HttpStatusCode statusCode) : base(message)
        {
            HttpStatusCode = statusCode;
        }

        public ServiceException(ILogger logger, string message)
            : base(message)
        {
            logger.LogError(message);
        }

        public ServiceException(string message, Exception exception)
            : base(message, exception)
        {
        }
        public ServiceException(ILogger logger, string message, Exception exception)
        : base(message, exception)
        {
            logger.LogError(exception, message);
        }
    }
}
