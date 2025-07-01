using System.Net;

namespace CustomerCustomerApi.Exceptions
{
    public class RBACException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public RBACException()
            : base() { }

        public RBACException(string message) : base(message) { }

        public RBACException(string message, HttpStatusCode statusCode) : base(message) 
        { 
            HttpStatusCode = statusCode;
        }

        public RBACException(ILogger logger, string message)
            : base(message)
        {
            logger.LogError(message);
        }

        public RBACException(string message, Exception exception)
            : base(message, exception)
        {
        }
        public RBACException(ILogger logger, string message, Exception exception)
        : base(message, exception)
        {
            logger.LogError(exception, message);
        }
    }
}
