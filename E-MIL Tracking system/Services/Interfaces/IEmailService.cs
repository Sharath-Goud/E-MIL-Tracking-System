using E_MIL_Tracking_system.DTOs;

namespace E_MIL_Tracking_system.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);

        Task SendEmailWithInlineImagesAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? beforeImageFullPath,
            string? afterImageFullPath
        );

        Task SendEmailWithMultipleInlineImagesAsync(
            string toEmail,
            string subject,
            string htmlBody,
            List<InlineEmailImage> inlineImages
        );
    }
}