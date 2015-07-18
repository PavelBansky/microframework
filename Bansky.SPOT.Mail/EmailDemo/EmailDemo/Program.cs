using System;
using Microsoft.SPOT;
using Bansky.SPOT.Mail;

namespace EmailDemo
{
    public class Program
    {
        public static void Main()
        {

            MailMessage message = new MailMessage();
            // Set sender name and address
            message.From = new MailAddress("foo.bar@contoso.com", "Foo Bar");

            // Set recipient address            
            message.To.Add(new MailAddress("john@doe.net", "John Doe"));
            message.Cc.Add(new MailAddress("manager@contoso.com", "The Boss"));

            message.Subject = "Hello World";
            message.Body = "Good news,<br />from now on you can send e-mails from <b>.NET Micro Framework</b>.";
            // Format body as HTML
            message.IsBodyHtml = true;

            // Create new attachment and define it's name
            Attachment attachment = new Attachment("Snowflake.gif");
            attachment.ContentType = "image/gif";
            attachment.TransferEncoding = TransferEncoding.Base64;
            // Attachment content
            attachment.Content = Base64.Encode(Resources.GetBytes(
                                               Resources.BinaryResources.Snowflake_gif),
                                               true);

            // Add attachment to message
            message.Attachments.Add(attachment);

            // Create new SMTP instance
            SmtpClient smtp = new SmtpClient("smtp.contoso.com", 25);
            try
            {
                // Authenicate to server
                smtp.Authenticate = true;
                smtp.Username = "username";
                smtp.Password = "password";

                // Send message
                smtp.Send(message);
            }
            catch (SmtpException e)
            {
                // Exception handling here 
                Debug.Print(e.Message);
                Debug.Print("Error Code: " + e.ErrorCode.ToString());
            }
            finally
            {
                smtp.Dispose();
            }
        }
    }
}