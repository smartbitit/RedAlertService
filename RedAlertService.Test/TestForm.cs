using Azure.Identity;
using Microsoft.Graph;
using models = Microsoft.Graph.Models;
using Microsoft.Graph.Core;
using Microsoft.Identity.Client;

namespace RedAlertService.Test
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }


        private async void button_SendTestEmail_Click(object sender, EventArgs e)
        {
            try
            {
                Common.Logging.LoggingService.LogInformation("Sending test Email....");
                var emailHelper = new Email.EmailHelper();
                await emailHelper.SendEmail(
                    "Redalerts@redthorn.com",
                    new List<string>() { "satyabratajena@gmail.com", "satyatjena@gmail.com", "satya.jena@smartbitit.com" },
                    "Redthorn Test Email",
                    "Welcome to RedAlert Services.",
                    String.Empty,
                    null);
                Common.Logging.LoggingService.LogInformation("Test Email sent....");
            }
            catch (Exception ex)
            {
                Common.Logging.LoggingService.LogError("Error Sending Email", ex);
            }
        }

        private async void button_ProcessEmailRequests_Click(object sender, EventArgs e)
        {
            try
            {
                var emailController = new Email.EmailController();
                MessageBox.Show("Starting Process");
                await emailController.ProcessEmailRequests();
                MessageBox.Show(Email.EmailController.Messages);
                Email.EmailController.ClearMessages();
            }
            catch (Exception ex)
            {
                Common.Logging.LoggingService.LogError("Error Sending Email", ex);
            }
        }
    }
}