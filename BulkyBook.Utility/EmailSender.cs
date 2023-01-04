using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        //generic email sender Mimekite ve mailkit nuget packagelerini indirdik.
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailtoSend = new MimeMessage();
            emailtoSend.From.Add(MailboxAddress.Parse("barankalakoglu7@gmail.com"));
            emailtoSend.To.Add(MailboxAddress.Parse(email));
            emailtoSend.Subject = subject;
            emailtoSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage};

            //send email,
            using (var emailClient = new SmtpClient())
            {
                //smtp servera ve port numarasına ihtiyacımız var. Biz googleınkini kullandık.
                emailClient.Connect("smpt.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("barankalakoglu7@gmail.com", "Baranbaba1.");
                emailClient.Send(emailtoSend);
                emailClient.Disconnect(true); //its always good idea to disconnect your email client
            }

            return Task.CompletedTask;
        }
    }
    
}
