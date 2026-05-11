using System.Net;
using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Services;

namespace E_MIL_Tracking_system.Services
{
    public class RccaReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RccaReminderBackgroundService> _logger;

        public RccaReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<RccaReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendRccaReminderMailsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending RCCA reminder mails");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task SendRccaReminderMailsAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var checklistService = scope.ServiceProvider.GetRequiredService<ChecklistService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var pendingRecords = await checklistService.GetPendingRccaReminderRecordsAsync();

            foreach (var item in pendingRecords)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                string subject = $"RCCA Reminder - MIL Audit Point Pending - {item.Date:dd-MM-yyyy}";

                string problemStatement = string.IsNullOrWhiteSpace(item.ProblemStatement)
                ? "-"
                : string.Join("<br/>",
                    Enumerable.Range(0, (int)Math.Ceiling(item.ProblemStatement.Length / 50.0))
                    .Select(i => WebUtility.HtmlEncode(item.ProblemStatement.Substring(
                        i * 50,
                        Math.Min(50, item.ProblemStatement.Length - (i * 50))
                    ))));

                string body = $@"
                    <html>
                    <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>

                        <div style='background:#ffffff; padding:20px; border-radius:12px;'>

                            <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                                RCCA Reminder
                            </h2>

                            <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>
                            <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                            <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                                RCCA has not been updated for the below MIL audit point even after 6 hours from creation time.
                                Please update the RCCA immediately.
                            </p>

                            <table cellpadding='0' cellspacing='0'
                                   style='border-collapse:collapse; width:100%; min-width:1600px; font-size:12px; text-align:center;'>

                                <thead>
                                    <tr style='background:#d7e5e1; color:#111827;'>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>S.NO</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Section</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Station Name</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Issue Type</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Problem Statement</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Frequency</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Issue Severity</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Category</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Starting Date</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Due Date</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>CM DRI</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Respective Department</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Apple DRI</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Type of Audit</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Auditor</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Before Image</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'> RCCA </th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Status</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>1</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.Section)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.StationName)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.IssueType)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; max-width:260px; word-wrap:break-word; white-space:normal; line-height:1.6;'>
                                            {problemStatement}
                                        </td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.Frequency)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.IssueSeverity)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.Category)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Date?.ToString("dd-MM-yyyy")}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{item.DueDate?.ToString("dd-MM-yyyy")}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.CmDri)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.RespectiveDepartment)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.AppleDri)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.TypeOfAudit)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.Auditor)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.BeforeImagePath)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{WebUtility.HtmlEncode(item.Rcca)}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>
                                            {WebUtility.HtmlEncode(item.Status)}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </body>
                    </html>";

                try
                {
                    await emailService.SendEmailWithMultipleInlineImagesAsync(
                        "Sharath_G@foxlink.com",
                        subject,
                        body,
                        new List<InlineEmailImage>()
                    );

                    await checklistService.MarkRccaReminderMailSentAsync(item.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send RCCA reminder mail for Checklist Id {Id}", item.Id);
                }
            }
        }
    }
}