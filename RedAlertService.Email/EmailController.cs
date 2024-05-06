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
                        try
                        {
                            MoveEmailMSALToFailed(processor_id);
                        }
                        catch (Exception ex2)
                        {
                            Common.Logging.LoggingService.LogError("Failed to move from MSAL to MSAL_FAILED.", ex2);
                        }
                        

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
                    Common.Logging.LoggingService.LogInformation($@"Sender: {emailRequest.Sender} | Recipients #: {emailRequest.Recipients} | Subject #: {emailRequest.Subject} | EventNo #: {emailRequest.EventNo}");

                    counter++;
                    await emailHelper.SendEmail(
                        emailRequest.Sender,
                        emailRequest.Recipients.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList(),
                        emailRequest.Subject,
                        emailRequest.Message,
                        emailRequest.AttachmentName,
                        emailRequest.Attachment, 
                        EmailTemplateMap.GetEmailTemplate(emailRequest.EventNo, emailRequest.NonEventNo));

                    DeleteFromEmailMSAL(emailRequest.MSALId);
                }
                #endregion Send Email  and Delete Record from Database
            } while (emailRequests.Count > 0);
        }

        private void MoveEmailMSALToFailed(int processor_id)
        {
            string insertQuery = @$"INSERT INTO SFMAIL_MSAL_FAILED(MSAL_ID, EVENT_NO, SUBJECT, SENDER, RECIPIENTS, MESSAGE, NON_EVENT_NO, ATTACHMENT_NAME, ATTACHMENT) SELECT MSAL_ID, EVENT_NO, SUBJECT, SENDER, RECIPIENTS, MESSAGE, NON_EVENT_NO, ATTACHMENT_NAME, ATTACHMENT FROM SFMAIL_MSAL WHERE (PROCESSOR_ID = {processor_id})";
            
            string deleteQuery = @$"DELETE FROM SFMAIL_MSAL WHERE (PROCESSOR_ID = {processor_id})";

            using (OracleConnection connection = new OracleConnection(DBConnection.GetConnectionString()))
            {
                connection.Open();
                using (OracleTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (OracleCommand insertCommand = new OracleCommand(insertQuery, connection))
                        using (OracleCommand deleteCommand = new OracleCommand(deleteQuery, connection))
                        {
                            insertCommand.Transaction = transaction;
                            deleteCommand.Transaction = transaction;

                            insertCommand.ExecuteNonQuery();
                            deleteCommand.ExecuteNonQuery();

                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Common.Logging.LoggingService.LogError(ex.Message, ex);
                        throw;
                    }
                }
            }
        }

        private void DeleteFromEmailMSAL(int msalId)
        {
            string deleteQuery = @$"DELETE FROM SFMAIL_MSAL WHERE (MSAL_ID = {msalId})";
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
    }
}
