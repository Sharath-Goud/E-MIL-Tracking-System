using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Services;

public class ApprovalReviewReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ApprovalReviewReminderService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var checklistService = scope.ServiceProvider.GetRequiredService<ChecklistService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var pendingList = await checklistService.GetPendingReviewReminderAsync();

            foreach (var savedRecord in pendingList)
            {
                if (string.IsNullOrWhiteSpace(savedRecord.Email))
                    continue;

                var inlineImages = new List<InlineEmailImage>();

                string beforeImageHtml = BuildInlineImagesHtml(
                    savedRecord.BeforeImagePath,
                    "before",
                    inlineImages
                );

                string afterImageHtml = BuildInlineImagesHtml(
                    savedRecord.AfterImagePath,
                    "after",
                    inlineImages
                );

                string subject = $"Reminder: MIL Audit Review Pending - {DateTime.Now:dd-MM-yyyy}";

                string body = $@"
                    <html>
                    <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>

                        <div style='background:#ffffff; padding:20px; border-radius:12px;'>

                            <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                                MIL Audit Review Reminder
                            </h2>

                            <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>

                            <p style='margin:0 0 16px; font-size:14px; line-height:1.6;'>
                                The MIL Audit Review Required mail was already sent,
                                but no Accept or Reject response has been received yet.
                            </p>

                            <p style='margin:0 0 20px; font-size:14px; line-height:1.6; font-weight:600; color:#b45309;'>
                                Please take the necessary action for the below audit point.
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
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>RCCA</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Starting Date</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Due Date</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>CM DRI</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Apple DRI</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Type of Audit</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Before Image</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>After Image</th>
                                        <th style='border:1px solid #c4cfd4; padding:10px;'>Status</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>1</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Section}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.StationName}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.IssueType}</td>

                                        <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; max-width:260px; word-wrap:break-word; white-space:normal; line-height:1.6;'>
                                            {savedRecord.ProblemStatement}
                                        </td>

                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Frequency}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.IssueSeverity}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Category}</td>

                                        <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; font-weight:600;'>
                                            {(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}
                                        </td>

                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Date?.ToString("dd-MM-yyyy")}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.CmDri}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.AppleDri}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.TypeOfAudit}</td>

                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{beforeImageHtml}</td>
                                        <td style='border:1px solid #e5e7eb; padding:10px;'>{afterImageHtml}</td>

                                        <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>
                                            {savedRecord.Status}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>

                        </div>

                    </body>
                    </html>";

                await emailService.SendEmailWithMultipleInlineImagesAsync(
                    savedRecord.Email,
                    subject,
                    body,
                    inlineImages
                );

                await checklistService.MarkReviewReminderSentAsync(
                    savedRecord.ChecklistId,
                    savedRecord.Email
                );
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private string BuildInlineImagesHtml(
        string? imagePathText,
        string prefix,
        List<InlineEmailImage> inlineImages)
    {
        if (string.IsNullOrWhiteSpace(imagePathText))
            return "-";

        string html = "";

        var imagePaths = imagePathText
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();

        foreach (var imagePath in imagePaths)
        {
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!File.Exists(fullPath))
                continue;

            string contentId = prefix + "_" + Guid.NewGuid().ToString("N");

            inlineImages.Add(new InlineEmailImage
            {
                ContentId = contentId,
                FilePath = fullPath
            });

            html += $@"
                <div style='display:inline-block; margin:4px;'>
                    <img src='cid:{contentId}'
                         width='80'
                         height='60'
                         style='object-fit:cover;border-radius:6px;border:1px solid #d1d5db;' />
                </div>";
        }

        return string.IsNullOrWhiteSpace(html) ? "-" : html;
    }
}