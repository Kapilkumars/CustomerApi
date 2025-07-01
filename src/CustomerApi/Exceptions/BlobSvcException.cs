using CustomerCustomerApi.Services;

namespace CustomerCustomerApi.Exceptions;

public class BlobSvcException : Exception
{
    public BlobSvcException(ILogger<BlobSvc> logger, string message, Exception exception)
        : base(message, exception)
    {
        logger.LogError(exception, message);
    }
}