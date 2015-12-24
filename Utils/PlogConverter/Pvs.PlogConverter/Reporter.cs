using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProgramVerificationSystems.PlogConverter
{
    class Reporter
    {
        public string Header { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string FromAddress { get; set; }
        public bool SendEmail { get; set; }

        private Reporter()
        {
        }
        static Reporter()
        {
            Instance = new Reporter();
        }
        public static Reporter Instance { get; private set; }

        public void SendEmails(string author, string file, List<string> emails, List<string> adminEmails)
        {
            if (!SendEmail)
                return;
            try
            {
                MailMessage message = new MailMessage()
                {
                    From = new MailAddress(FromAddress),
                    Subject = Header,
                    IsBodyHtml = true
                };
                using (StreamReader reader = File.OpenText(file))
                {
                    message.Body = reader.ReadToEnd();
                }
                foreach (string email in emails)
                {
                    if (email == "none")
                        continue;
                    message.To.Add(email);
                }
                foreach (string adminEmail in adminEmails)
                {
                    if (adminEmail == "none")
                        continue;
                    message.Bcc.Add(adminEmail);
                }

                using(SmtpClient client = new SmtpClient(Server, Port))
                {
                    client.Credentials = new NetworkCredential(SmtpUser, SmtpPassword);
                    client.Send(message);
                }
                message.Dispose();
            }
            catch (Exception)
            {
                ;
            }
        }
    }
}
