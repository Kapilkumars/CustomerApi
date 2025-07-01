using System.Net;

namespace CustomerCustomerApi.Exceptions
{
    public class CustomerSvcException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public CustomerSvcException()
            : base() { }

        public CustomerSvcException(string message) : base(message) { }

        public CustomerSvcException(string message, HttpStatusCode statusCode) : base(message)
        {
            HttpStatusCode = statusCode;
        }

        public CustomerSvcException(ILogger logger, string message)
            : base(message)
        {
            logger.LogError(message);
        }

        public CustomerSvcException(string message, HttpStatusCode statusCode, Exception exception)
            : base(message, exception)
        {
            HttpStatusCode = statusCode;
        }
        public CustomerSvcException(ILogger logger, string message, Exception exception)
        : base(message, exception)
        {
            logger.LogError(exception, message);
        }
    }
}
