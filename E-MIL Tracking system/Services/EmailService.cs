using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace E_MIL_Tracking_system.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _smtpSettings.SenderName,
                _smtpSettings.SenderEmail
            ));

            foreach (var emailAddress in toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                email.To.Add(MailboxAddress.Parse(emailAddress.Trim()));
            }
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _smtpSettings.Host,
                _smtpSettings.Port,
                SecureSocketOptions.None
            );

            //if (!string.IsNullOrWhiteSpace(_smtpSettings.Username) &&
            //    !string.IsNullOrWhiteSpace(_smtpSettings.Password))
            //{
            //    await smtp.AuthenticateAsync(
            //        _smtpSettings.Username,
            //        _smtpSettings.Password
            //    );
            //}

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendEmailWithInlineImagesAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? beforeImageFullPath,
            string? afterImageFullPath)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _smtpSettings.SenderName,
                _smtpSettings.SenderEmail
            ));

            foreach (var emailAddress in toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                email.To.Add(MailboxAddress.Parse(emailAddress.Trim()));
            }
            email.Subject = subject;

            var builder = new BodyBuilder();

            if (!string.IsNullOrWhiteSpace(beforeImageFullPath) && File.Exists(beforeImageFullPath))
            {
                var beforeImage = builder.LinkedResources.Add(beforeImageFullPath);
                beforeImage.ContentId = "beforeImageCid";

                htmlBody = htmlBody.Replace(
                    "BEFORE_IMAGE_PLACEHOLDER",
                    "<a href='cid:beforeImageCid' target='_blank'><img src='cid:beforeImageCid' style='width:90px;height:90px;object-fit:cover;border-radius:6px;display:block;margin:auto;border:1px solid #d1d5db;' /></a>"
                );
            }
            else
            {
                htmlBody = htmlBody.Replace("BEFORE_IMAGE_PLACEHOLDER", "-");
            }

            if (!string.IsNullOrWhiteSpace(afterImageFullPath) && File.Exists(afterImageFullPath))
            {
                var afterImage = builder.LinkedResources.Add(afterImageFullPath);
                afterImage.ContentId = "afterImageCid";

                htmlBody = htmlBody.Replace(
                    "AFTER_IMAGE_PLACEHOLDER",
                    "<a href='cid:afterImageCid' target='_blank'><img src='cid:afterImageCid' style='width:90px;height:90px;object-fit:cover;border-radius:6px;display:block;margin:auto;border:1px solid #d1d5db;' /></a>"
                );
            }
            else
            {
                htmlBody = htmlBody.Replace("AFTER_IMAGE_PLACEHOLDER", "-");
            }

            builder.HtmlBody = htmlBody;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                 _smtpSettings.Host,
                 _smtpSettings.Port,
                 SecureSocketOptions.None
             );

            //if (!string.IsNullOrWhiteSpace(_smtpSettings.Username) &&
            //    !string.IsNullOrWhiteSpace(_smtpSettings.Password))
            //{
            //    await smtp.AuthenticateAsync(
            //        _smtpSettings.Username,
            //        _smtpSettings.Password
            //    );
            //}

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}