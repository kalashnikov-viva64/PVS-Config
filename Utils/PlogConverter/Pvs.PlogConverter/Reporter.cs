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

        public void SendEmails(string author, string file, List<string> emails)
        {
            if (!SendEmail)
                return;
            try
            {
                SmtpClient client = new SmtpClient(Server, Port)
                {
                    Credentials = new NetworkCredential(SmtpUser, SmtpPassword)
                };

                foreach (string email in emails)
                {
                    if (email == "none")
                        continue;

                    using (StreamReader reader = File.OpenText(file))
                    {
                        MailMessage message = new MailMessage(FromAddress, email)
                        {
                            Subject = Header,
                            Body = reader.ReadToEnd(),
                            IsBodyHtml = true
                        };
                        client.Send(message);
                        message.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
    }
}
