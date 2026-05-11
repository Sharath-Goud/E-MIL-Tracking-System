using E_MIL_Tracking_system.DTOs;
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

            try
            {
                smtp.Timeout = 60000;

                await smtp.ConnectAsync(
                    _smtpSettings.Host,
                    _smtpSettings.Port,
                    SecureSocketOptions.None
                );

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);
            }
            catch
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(false);
                }

                throw;
            }
        }

        public async Task SendEmailWithInlineImagesAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? beforeImageFullPath)
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

            builder.HtmlBody = htmlBody;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();

            try
            {
                smtp.Timeout = 60000;

                await smtp.ConnectAsync(
                    _smtpSettings.Host,
                    _smtpSettings.Port,
                    SecureSocketOptions.None
                );

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);
            }
            catch
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(false);
                }

                throw;
            }
        }

        public async Task SendEmailWithInlineImagesAsync1(
            string toEmail,
            string subject,
            string htmlBody,
            string? beforeImageFullPath,
            string? afterImageFullPath)
        {
            var inlineImages = new List<InlineEmailImage>();

            if (!string.IsNullOrWhiteSpace(beforeImageFullPath) && System.IO.File.Exists(beforeImageFullPath))
            {
                inlineImages.Add(new InlineEmailImage
                {
                    ContentId = "beforeImage",
                    FilePath = beforeImageFullPath
                });

                htmlBody = htmlBody.Replace(
                    "BEFORE_IMAGE_PLACEHOLDER",
                    "<img src='cid:beforeImage' width='80' height='60' style='object-fit:cover;border-radius:6px;border:1px solid #d1d5db;' />"
                );
            }
            else
            {
                htmlBody = htmlBody.Replace("BEFORE_IMAGE_PLACEHOLDER", "-");
            }

            if (!string.IsNullOrWhiteSpace(afterImageFullPath) && System.IO.File.Exists(afterImageFullPath))
            {
                inlineImages.Add(new InlineEmailImage
                {
                    ContentId = "afterImage",
                    FilePath = afterImageFullPath
                });

                htmlBody = htmlBody.Replace(
                    "AFTER_IMAGE_PLACEHOLDER",
                    "<img src='cid:afterImage' width='80' height='60' style='object-fit:cover;border-radius:6px;border:1px solid #d1d5db;' />"
                );
            }
            else
            {
                htmlBody = htmlBody.Replace("AFTER_IMAGE_PLACEHOLDER", "-");
            }

            await SendEmailWithMultipleInlineImagesAsync(
                toEmail,
                subject,
                htmlBody,
                inlineImages
            );
        }

        public async Task SendEmailWithMultipleInlineImagesAsync(
        string toEmail,
        string subject,
        string htmlBody,
        List<InlineEmailImage> inlineImages)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _smtpSettings.SenderName,
                _smtpSettings.SenderEmail
            ));

            foreach (var email in toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(MailboxAddress.Parse(email.Trim()));
            }

            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            foreach (var img in inlineImages)
            {
                if (!string.IsNullOrWhiteSpace(img.FilePath) && File.Exists(img.FilePath))
                {
                    var image = builder.LinkedResources.Add(img.FilePath);
                    image.ContentId = img.ContentId;
                    image.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                }
            }

            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                client.Timeout = 60000;

                await client.ConnectAsync(
                    _smtpSettings.Host,
                    _smtpSettings.Port,
                    SecureSocketOptions.None
                );

                if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
                {
                    await client.AuthenticateAsync(
                        _smtpSettings.Username,
                        _smtpSettings.Password
                    );
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(false);
                }

                throw;
            }
        }
    }
}