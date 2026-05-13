using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Services;

public class AfterImageReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundEmailQueue _emailQueue;
    private readonly IWebHostEnvironment _env;

    public AfterImageReminderService(
    IServiceScopeFactory scopeFactory,
    IBackgroundEmailQueue emailQueue,
    IWebHostEnvironment env)
    {
        _scopeFactory = scopeFactory;
        _emailQueue = emailQueue;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var checklistService = scope.ServiceProvider.GetRequiredService<ChecklistService>();

            var records = await checklistService.GetPendingAfterImageReminderRecordsAsync();

            foreach (var item in records)
            {
                string subject = $"After Image Reminder - {DateTime.Now:dd-MM-yyyy}";

                var inlineImages = new List<InlineEmailImage>();
                string beforeImageHtml = "-";

                if (!string.IsNullOrWhiteSpace(item.BeforeImagePath))
                {
                    var imagePaths = item.BeforeImagePath
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    beforeImageHtml = "";

                    foreach (var imagePath in imagePaths)
                    {
                        string fullPath = Path.Combine(
                            _env.WebRootPath,
                            imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                        );

                        if (File.Exists(fullPath))
                        {
                            string contentId = "before_" + Guid.NewGuid().ToString("N");

                            inlineImages.Add(new InlineEmailImage
                            {
                                ContentId = contentId,
                                FilePath = fullPath
                            });

                            beforeImageHtml += $@"
                <div style='display:inline-block; margin:4px;'>
                    <img src='cid:{contentId}'
                         width='80'
                         height='60'
                         style='object-fit:cover;border-radius:6px;border:1px solid #d1d5db;' />
                </div>";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(beforeImageHtml))
                    {
                        beforeImageHtml = "-";
                    }
                }

                string body = $@"
                <html>
                <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>
                    <div style='background:#ffffff; padding:20px; border-radius:12px;'>
                        <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                            After Image Reminder
                        </h2>

                        <p>Hi Team,</p>
                        <p>Good day.</p>

                        <p>
                            After Image has not been uploaded for the below MIL audit point even after 4 hours from RCCA update time.
                            Please upload the After Image immediately.
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
                                    <th style='border:1px solid #c4cfd4; padding:10px;'>Auditor</th>
                                    <th style='border:1px solid #c4cfd4; padding:10px;'>Before Image</th>
                                    <th style='border:1px solid #c4cfd4; padding:10px;'>Status</th>
                                </tr>
                            </thead>

                            <tbody>
                                <tr>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>1</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Section}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.StationName}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.IssueType}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left;'>{item.ProblemStatement}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Frequency}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.IssueSeverity}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Category}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; font-weight:600;'>{item.Rcca}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Date?.ToString("dd-MM-yyyy")}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.DueDate?.ToString("dd-MM-yyyy")}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.CmDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.AppleDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.TypeOfAudit}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Auditor}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{beforeImageHtml}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>{item.Status}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </body>
                </html>";

                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                _emailQueue.QueueEmail(async ct =>
                {
                    await emailService.SendEmailWithMultipleInlineImagesAsync(
                        "Sharath_G@foxlink.com",
                        subject,
                        body,
                        inlineImages
                    );
                });

                await checklistService.MarkAfterImageReminderSentAsync(item.Id);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}