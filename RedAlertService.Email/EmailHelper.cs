using Azure.Identity;
using Microsoft.Graph;
using models = Microsoft.Graph.Models;
using Microsoft.Graph.Core;
using Microsoft.Identity.Client;

namespace RedAlertService.Email
{
    public class EmailHelper
    {
        public async Task SendEmail(
            string sender, 
            List<string> recipients, 
            string subject, 
            string message, 
            string? attachmentName, 
            byte[]? attachment, 
            string htmlTemplateFileName = "redthorn_generic_template.html")
        {
            var configuration = EmailCredential.GetEmailCredential(sender.Split('@')[1]);
            if (configuration == null)
                return;

            var credentials = new ClientSecretCredential(
                configuration.TenantId,
                configuration.ClientId,
                configuration.ClientSecret,
                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

            var graphServiceClient = new GraphServiceClient(credentials);
            string htmlTemplatePath = $"html_templates/{htmlTemplateFileName}";
            string htmlTemplateContent = File.ReadAllText(htmlTemplatePath);
            htmlTemplateContent = htmlTemplateContent.Replace("{{message}}", message);

            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = new models.Message
                {
                    Subject = subject,
                    Body = new models.ItemBody
                    {
                        ContentType = models.BodyType.Html,
                        Content = htmlTemplateContent
                    },
                    ToRecipients = recipients.Select((r) => new models.Recipient { EmailAddress = new models.EmailAddress { Address = r } }).ToList()
                },
                SaveToSentItems= false
            };

            var user = graphServiceClient
                .Users[sender];

            await user.SendMail.PostAsync(requestBody);
        }
    }
}
