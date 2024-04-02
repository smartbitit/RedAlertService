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
        public async Task ProcessEmailRequests(CancellationToken stoppingToken, int processor_id)
        {
            var emailRequests = new List<EmailRequest>();
            var emailHelper = new EmailHelper();

            do
            {
                if (stoppingToken.IsCancellationRequested)
                    break; // Exit the loop gracefully

                emailRequests = new List<EmailRequest>();
                #region Read Email Requests
                using (OracleConnection connection = new OracleConnection(DBConnection.GetConnectionString()))
                {
                    try
                    {
                        connection.Open();

                        string updateQuery =
$@"UPDATE 
    SFMAIL_MSAL
SET 
    PROCESSOR_ID = {processor_id}
WHERE 
    MSAL_ID = (
        SELECT 
            MIN(MSAL_ID) KEEP (DENSE_RANK FIRST ORDER BY MSAL_ID)
        FROM 
            SFMAIL_MSAL
        WHERE 
            PROCESSOR_ID IS NULL
        )
    AND 
    NOT EXISTS (
                SELECT 1
                FROM SFMAIL_MSAL
                WHERE PROCESSOR_ID = {processor_id}
        )";
                        string selectQuery = 
@$"SELECT 
    MSAL_ID, 
    EVENT_NO, 
    SUBJECT, 
    SENDER, 
    RECIPIENTS, 
    MESSAGE, 
    NON_EVENT_NO, 
    ATTACHMENT_NAME, 
    ATTACHMENT 
FROM 
    SFMAIL_MSAL
WHERE
    (PROCESSOR_ID = {processor_id})";
                        using (OracleCommand command = new OracleCommand(selectQuery, connection))
                        {
                            // Execute update statement
                            using (OracleCommand updateCommand = new OracleCommand(updateQuery, connection))
                            {
                                int rowsUpdated = updateCommand.ExecuteNonQuery();
                                Console.WriteLine($"Rows updated: {rowsUpdated}");
                            }

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
                    }
                }
                #endregion Read Email Requests

                #region Send Email and Delete Record from Database
                var counter = 0;
                foreach (var emailRequest in emailRequests.Where((r) => (!string.IsNullOrWhiteSpace(r.Sender)) && (!string.IsNullOrWhiteSpace(r.Recipients))))
                {
                    if (stoppingToken.IsCancellationRequested)
                        break; // Exit the loop gracefully

                    Common.Logging.LoggingService.LogInformation($@"Sending Email >> Processor #: {processor_id} | Request #: {emailRequest.MSALId}");

                    counter++;
                    await emailHelper.SendEmail(
                        emailRequest.Sender,
                        emailRequest.Recipients.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList(),
                        emailRequest.Subject,
                        emailRequest.Message,
                        emailRequest.AttachmentName,
                        emailRequest.Attachment);

                    string deleteQuery = @$"DELETE FROM SFMAIL_MSAL WHERE (MSAL_ID = {emailRequest.MSALId})";
                    using (OracleConnection connection = new OracleConnection(DBConnection.GetConnectionString()))
                    {
                        using (OracleCommand command = new OracleCommand(deleteQuery, connection))
                        {
                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Common.Logging.LoggingService.LogError(ex.Message, ex);
                            }
                        }
                    }
                }
                #endregion Send Email  and Delete Record from Database
            } while (emailRequests.Count > 0);
        }
    }
}
