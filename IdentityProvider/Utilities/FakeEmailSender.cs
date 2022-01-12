using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace NMSI.IdentityProvider.Utilities
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
