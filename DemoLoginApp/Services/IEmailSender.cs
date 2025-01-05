using Microsoft.AspNetCore.Identity.UI.Services;

namespace DemoLoginApp.Services
{
    public interface ICustomEmailSender : IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
