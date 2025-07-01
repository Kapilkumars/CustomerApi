using System.Net;

namespace CustomerCustomerApi.Exceptions
{
    public class NotFoundExeption : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.NotFound;
        public NotFoundExeption(string message) : base(message) { }
    }
}
