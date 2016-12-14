using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace sendMail
{
    public class Program
    {
        public static string Username = "dicomdaemon";
        public static string Password = null;
        public const string SmtpServer = "rsmail020.skane.se";

        private static string From = Username + "@skane.se";

        static void Main(string[] args)
        {
            send("rickard.cronholm@skane.se", "test", "more text");
        }

        public static void send(string recipient, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient();
            NetworkCredential basicCredentials = new NetworkCredential(Username, Password, SmtpServer);
            MailMessage message = new MailMessage();
            MailAddress fromAdress = new MailAddress(From);

            // setup the host, increase the timeout to 5 minutes
            smtpClient.Host = SmtpServer;
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredentials;
            smtpClient.Timeout = (60 * 5 * 1000);

            message.From = fromAdress;
            message.Subject = subject + " - " + DateTime.Now.Date.ToString().Split(' ')[0];
            message.Body = body.Replace("\r\n", "<br>");
            message.IsBodyHtml = true;
            message.To.Add(recipient);

            smtpClient.Send(message);
        }
    }
}
