namespace CustomerCustomerApi.Interfaces
{
    public interface IAuthToUserProvider
    {
        string GraphUserId { get; }
        Task<string> GetUserIdAsync();
    }
}
