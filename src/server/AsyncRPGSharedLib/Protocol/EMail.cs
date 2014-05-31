using System;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Net.Mail;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGSharedLib.Protocol
{
    public class EMail
    {
        public static bool isWellFormedAddress(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);

            return re.IsMatch(inputEmail);
        }

        public static bool SendMessage(
            string targetAddress,
            string subject,
            string body)
        {
            bool success = true;

            MailMessage msg = new MailMessage();

            msg.From = new MailAddress(MailConstants.WEB_SERVICE_EMAIL_ADDRESS);
            msg.To.Add(targetAddress);
            msg.Body = body;
            msg.Subject = subject;
            msg.BodyEncoding = System.Text.Encoding.ASCII;
            msg.IsBodyHtml = true;

            try
            {
                SmtpClient smtpClient = new SmtpClient();

                smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                smtpClient.EnableSsl = true;
                smtpClient.SendAsync(msg, subject); // Identify the async message sent by the subject
            }
            catch (System.Exception)
            {
                success = false;
            }

            return success;
        }

        public static bool SendVerifyAccountMessage(
            string targetAddress,
            string webServiceURL,
            string username,
            string emailVerificationCode)
        {
            string verifyUrl =
                string.Format("{0}AccountEmailVerifyRequest?username={1}&key={2}",
                    webServiceURL,
                    username,
                    emailVerificationCode);
            StringBuilder bodyBuilder = new StringBuilder(); // This message's body is scrawny. Gonna PUMP.. IT.. UP!

            bodyBuilder.Append("Please click the following link to verify the following information is correct for your account:<br/>");
            bodyBuilder.Append("<br/>");
            bodyBuilder.AppendFormat("Username: {0}<br/>", username);
            bodyBuilder.AppendFormat("E-mail: {0}<br/>", targetAddress);
            bodyBuilder.Append("<br/>");
            bodyBuilder.Append("Verification Link:<br/>");
            bodyBuilder.AppendFormat("<a href='{0}'>{0}</a>", verifyUrl);

            return SendMessage(targetAddress, "AsyncRPG account verification", bodyBuilder.ToString());
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                //Utilities.LogError(string.Format("E-mail({0}): Send Canceled", token));
            }

            if (e.Error != null)
            {
                //Utilities.LogError(string.Format("E-mail({0}): Error- {1}", token, e.Error.ToString()));
            }
        }
    }
}
