namespace CustomerCustomerApi.Interfaces;

public interface IEmailSvc
{
    Task<string> SendUserEmailAsync(string toUser, string userName, string tempPassword, string roles);
}
