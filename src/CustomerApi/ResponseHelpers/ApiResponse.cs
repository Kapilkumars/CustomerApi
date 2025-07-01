namespace CustomerCustomerApi.ResponseHelpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, int statusCode = 200)
        {
            return new ApiResponse<T> { Success = true, Data = data, StatusCode = statusCode };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode, string? details = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError { Message = message, Details = details },
                StatusCode = statusCode
            };
        }
    }

    public class ApiError
    {
        public string Message { get; set; }
        public string? Details { get; set; }
    }
}
