using System.Text.Json;

namespace RedAlertService.Email
{
    internal class EmailTemplateMap
    {
        private int  __eventNo;
        public int EventNo
        {
            get { return __eventNo; }
            set { __eventNo = value; }
        }

        private string __template = string.Empty;
        public string Template
        {
            get { return __template??(__template = string.Empty); }
            set { __template = value; }
        }

        private static bool EmailTemplateMapsRead = false;
        private static List<EmailTemplateMap> EmailTemplateMaps = new List<EmailTemplateMap>();
        public static string GetEmailTemplate(int? eventNo, int? nonEventNo)
        {
            if ((eventNo == null) || (eventNo <= 0))
                eventNo = nonEventNo;

            if ((eventNo == null) || (eventNo <= 0))
                return "redthorn_generic_template.html";

            if (!EmailTemplateMapsRead)
            {
                string filePath = Path.Combine(Common.IOHelper.GetAppPath(), "email_template_maps.json");
                Common.Logging.LoggingService.LogInformation(@$"Reading email template maps from {filePath}");
                string jsonData = File.ReadAllText(filePath);
                var emailTemplateMaps = JsonSerializer.Deserialize<List<EmailTemplateMap>>(jsonData);
                if (emailTemplateMaps != null)
                {
                    EmailTemplateMaps = emailTemplateMaps;
                    EmailTemplateMapsRead = true;
                }
            }

            var map = EmailTemplateMaps.FirstOrDefault((e) => e.EventNo == eventNo);

            return (map == null) ? "redthorn_generic_template.html" : map.Template;
        }
    }
}