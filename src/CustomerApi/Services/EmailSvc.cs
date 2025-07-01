using Customer.Metis.SettingsProviders.Interfaces;
using CustomerCustomerApi.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http.Headers;

namespace CustomerCustomerApi.Services;

public class EmailSvc : IEmailSvc
{
    ISendGridClient _sendGridClient;
    private readonly ILogger<EmailSvc> _logger;
    private readonly ISettingsProvider _settingsProvider;
    public EmailSvc(ILogger<EmailSvc> logger, ISendGridClient sendGridClient, ISettingsProvider settingsProvider)
    {
        _sendGridClient = sendGridClient;
        _logger = logger;
        _settingsProvider = settingsProvider;
    }
    public async Task<string> SendUserEmailAsync(string toUser, string userName, string tempPassword, string roles)
    {
        var message = new SendGridMessage();
        try
        {
            
            message.From = new EmailAddress("svet@digitaldotsinc.com", "Customer");
            message.SetSubject("Welcome to Customer Platform Services");
            message.AddTo(toUser);
            message.TemplateId = "d-b1584b39544e43eaab70fbe7eb24debf";
            message.SetTemplateData(new
            {
                first_name = userName,
                roles = roles,
                user = toUser,
                tempPassword = tempPassword,
                appUrl = _settingsProvider.GetSetting("MetisAppUrl")
            });
            var response = await _sendGridClient.SendEmailAsync(message);
            HttpHeaders headers = response.Headers;
            string id = "";

            //https://sendgrid.com/blog/the-nine-events-of-email/
            if (response.StatusCode == HttpStatusCode.Forbidden)
                _logger.LogError("Sending email with SendGrid failed. Issue with the account used. HttpStatusCode = Forbidden");

            if (headers.TryGetValues("X-Message-ID", out var values))
            {
                id = values.First();
            }

            _logger.LogInformation($"Email send successively using SendGrid. SendGrid X-Message-ID: {id}. EmailType: Registration, TemplateId: {message.TemplateId}");

            return id;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Sending email with SendGrid failed. TemplateId: {message.TemplateId}");
            throw;
        }

    }
}
