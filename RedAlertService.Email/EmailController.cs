using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedAlertService.Email
{
    public class EmailController
    {
        private static List<string> __messages = new List<string>();
        public static string Messages
        {
            get { return string.Join(Environment.NewLine, __messages); }
        }

        public static void ClearMessages()
        {
            __messages.Clear();
        }

        public async Task ProcessEmailRequests()
        {
            var emailRequests = new List<EmailRequest>();
            var emailHelper = new EmailHelper();

            do
            {
                emailRequests = new List<EmailRequest>();
                #region Read Email Requests
                __messages.Add($@"ConnectionString: {DBConnection.GetConnectionString()}");
                using (OracleConnection connection = new OracleConnection(DBConnection.GetConnectionString()))
                {
                    try
                    {
                        connection.Open();

                        string sqlQuery = "SELECT MSAL_ID, EVENT_NO, SUBJECT, SENDER, RECIPIENTS, MESSAGE, NON_EVENT_NO, ATTACHMENT_NAME, ATTACHMENT FROM SFMAIL_MSAL";
                        using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                        {
                            using (OracleDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var emailRequest = new EmailRequest()
                                    {
                                        MSALId= reader.GetInt32(reader.GetOrdinal("MSAL_ID")),
                                        EventNo = reader.IsDBNull(reader.GetOrdinal("EVENT_NO")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("EVENT_NO")),
                                        Subject = reader.GetString(reader.GetOrdinal("SUBJECT")),
                                        Sender = reader.GetString(reader.GetOrdinal("SENDER")),
                                        Recipients = reader.GetString(reader.GetOrdinal("RECIPIENTS")),
                                        Message = reader.GetString(reader.GetOrdinal("MESSAGE")),
                                        NonEventNo = reader.IsDBNull(reader.GetOrdinal("NON_EVENT_NO")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("NON_EVENT_NO")),
                                        AttachmentName = reader.IsDBNull(reader.GetOrdinal("ATTACHMENT_NAME")) ? null : reader.GetString(reader.GetOrdinal("ATTACHMENT_NAME"))
                                    };

                                    // Read the attachment blob data
                                    if (!string.IsNullOrWhiteSpace(emailRequest.AttachmentName))
                                        emailRequest.Attachment = (byte[])reader.GetValue(reader.GetOrdinal("ATTACHMENT"));

                                    emailRequests.Add(emailRequest);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Logging.LoggingService.LogError(ex.Message, ex);
                        __messages.Add(@$"Exception : {ex.Message}");
                    }
                }
                #endregion Read Email Requests

                __messages.Add(@$"Email Request Found: {emailRequests.Count}");

                #region Send Email and Delete Record from Database
                var counter = 0;
                foreach (var emailRequest in emailRequests.Where((r) => (!string.IsNullOrWhiteSpace(r.Sender)) && (!string.IsNullOrWhiteSpace(r.Recipients))))
                {
                    counter++;
                    __messages.Add(@$"Processing {counter} of {emailRequests.Count}");
                    await emailHelper.SendEmail(
                        emailRequest.Sender,
                        emailRequest.Recipients.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList(),
                        emailRequest.Subject,
                        emailRequest.Message,
                        emailRequest.AttachmentName,
                        emailRequest.Attachment);

                    string sqlQuery = @$"DELETE FROM SFMAIL_MSAL WHERE (MSAL_ID = {emailRequest.MSALId})";
                    using (OracleConnection connection = new OracleConnection(DBConnection.GetConnectionString()))
                    {
                        using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                        {
                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                __messages.Add(@$"Exception : {ex.Message}");
                            }
                        }
                    }


                }
                #endregion Send Email  and Delete Record from Database
            } while (emailRequests.Count > 0);
        }
    }
}
