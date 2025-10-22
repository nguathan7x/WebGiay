using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mime;

namespace WebGiayOnline.Services
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public GmailEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Gmail:Email"],
                    _configuration["EmailSettings:Gmail:AppPassword"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Gmail:Email"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

         

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] attachmentBytes, string attachmentName)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Gmail:Email"],
                    _configuration["EmailSettings:Gmail:AppPassword"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Gmail:Email"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            if (attachmentBytes != null && attachmentBytes.Length > 0)
            {
                var stream = new MemoryStream(attachmentBytes);
                var attachment = new Attachment(stream, attachmentName, MediaTypeNames.Application.Pdf);
                mailMessage.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}
