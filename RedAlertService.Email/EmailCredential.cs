using System.Text.Json;

namespace RedAlertService.Email
{
    internal class EmailCredential
    {
        private string __domain = string.Empty;
        public string Domain
        {
            get { return __domain?? (__domain = string.Empty); }
            set { __domain = value; }
        }

        private string __tenantId = string.Empty;
        public string TenantId
        {
            get { return __tenantId??(__tenantId = string.Empty); }
            set { __tenantId = value; }
        }

        private string __clientId = string.Empty;
        public string ClientId
        {
            get { return __clientId??(__clientId = string.Empty); }
            set { __clientId = value; }
        }

        private string __clientSecret = string.Empty;
        public string ClientSecret
        {
            get { return __clientSecret??(__clientSecret = string.Empty); }
            set { __clientSecret = value; }
        }

        private static bool EmailCredentialsRead = false;
        private static List<EmailCredential> EmailCredentials = new List<EmailCredential>();
        public static EmailCredential? GetEmailCredential(string domain) 
        {
            if (!EmailCredentialsRead)
            {
                string filePath = Path.Combine(Common.IOHelper.GetAppPath(), "email_credentials.json");
                Common.Logging.LoggingService.LogInformation(@$"Reading email credentials from {filePath}");
                string jsonData = File.ReadAllText(filePath);
                var emailCredentials = JsonSerializer.Deserialize<List<EmailCredential>>(jsonData);
                if (emailCredentials != null)
                {
                    EmailCredentials = emailCredentials;
                    EmailCredentialsRead = true;
                }
            }

            return EmailCredentials.FirstOrDefault((e) => e.Domain.Equals(domain, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}