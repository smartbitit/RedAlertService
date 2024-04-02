using Azure.Identity;
using Microsoft.Graph;
using models = Microsoft.Graph.Models;
using Microsoft.Graph.Core;
using Microsoft.Identity.Client;

namespace RedAlertService.Email
{
    public class EmailHelper
    {
        // Initialize the dictionary with common file extension to content type mappings
        private Dictionary<string, string> extensionToContentTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            // Add more mappings as needed
        };

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
            string htmlTemplatePath = Path.Combine(Common.IOHelper.GetAppPath(), "html_templates", htmlTemplateFileName);
            string htmlTemplateContent = File.ReadAllText(htmlTemplatePath);
            htmlTemplateContent = htmlTemplateContent.Replace("{{message}}", message);

            var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody();
            requestBody.Message = new models.Message
            {
                Subject = subject,
                Body = new models.ItemBody
                {
                    ContentType = models.BodyType.Html,
                    Content = htmlTemplateContent
                },
                ToRecipients = recipients.Select((r) => new models.Recipient { EmailAddress = new models.EmailAddress { Address = r } }).ToList(),
            };

            if (!string.IsNullOrWhiteSpace(attachmentName))
                requestBody.Message.Attachments = new List<models.Attachment>()
                    {
                        new models.FileAttachment
                        {
                            OdataType = "#microsoft.graph.fileAttachment",
                            Name = attachmentName,
                            ContentType = GetContentType(attachmentName),
                            ContentBytes = attachment
                        }
                    };

            requestBody.SaveToSentItems= false;

            var user = graphServiceClient
                .Users[sender];

            await user.SendMail.PostAsync(requestBody);
        }

        private string GetContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            string extension = System.IO.Path.GetExtension(fileName);
            extension = extension.ToLowerInvariant();

            return extensionToContentTypeMap.ContainsKey(extension)
                ? extensionToContentTypeMap[extension]
                : "application/octet-stream"; // Default content type if extension is not recognized
        }
    }
}
