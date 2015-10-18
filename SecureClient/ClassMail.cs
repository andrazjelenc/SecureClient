using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using S22.Imap;

namespace SecureClient
{
    class ClassMail
    {
        /// <summary>
        /// Send email using SMTP
        /// </summary>
        /// <param name="username">Username to connect to server(usually your email)</param>
        /// <param name="password">Password to connect</param>
        /// <param name="toMail">Receiver email address</param>
        /// <param name="subject">Mail subject</param>
        /// <param name="message">Mail body</param>
        /// <param name="host">SMTP host server</param>
        /// <param name="port">SMTP port</param>
        /// <param name="ssl">Secure sending</param>
        /// <returns>Sent! or Exception</returns>
        public static string Send(string username, string password, string toMail, string subject, string message, string host, int port, bool ssl)
        {
            MailMessage msg = new MailMessage();

            msg.From = new System.Net.Mail.MailAddress(username);
            msg.To.Add(toMail);
            msg.Subject = subject;
            msg.Body = message;

            SmtpClient client = new SmtpClient();
            client.Host = host; //smtp.gmail.com
            client.Port = port; // 587
            client.EnableSsl = ssl; //true
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);

            client.Timeout = 20000;

            try
            {
                client.Send(msg);
                return "Sent!";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            finally
            {
                msg.Dispose();
            }
        }

        /// <summary>
        /// Get mails from server IMAP
        /// </summary>
        /// <param name="username">Username to connect to server(usually your email)</param>
        /// <param name="password">Password to connect</param>
        /// <param name="hostIn">host server</param>
        /// <param name="portIn">port</param>
        /// <param name="secure">Secure receiving</param>
        /// <returns>List of mails [from, date, subject, body]</returns>
        public static List<string[]> getMails(string username, string password, string hostIn, int portIn, bool secure)
        {
            List<string[]> messagesList = new List<string[]>();

            using (ImapClient Client = new ImapClient(hostIn, portIn, username, password, AuthMethod.Login, secure))
            {
                //IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen());
                IEnumerable<uint> uids = Client.Search(SearchCondition.All());
                IEnumerable<MailMessage> messages = Client.GetMessages(uids);

                foreach (var message in messages)
                {
                    string from = message.From.Address.ToString().Trim();
                    string date = message.Date().ToString().Trim();
                    string subject = message.Subject.ToString().Trim();
                    string body = message.Body.ToString().Trim();

                    messagesList.Add(new string[] { from, subject, date, body });
                }
            }
            return messagesList;
        }
    }
}
