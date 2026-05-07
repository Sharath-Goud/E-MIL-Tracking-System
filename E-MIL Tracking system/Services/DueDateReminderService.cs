using E_MIL_Tracking_system.Services;

public class DueDateReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DueDateReminderService> _logger;

    public DueDateReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<DueDateReminderService> logger)
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
                using var scope = _scopeFactory.CreateScope();

                var checklistService = scope.ServiceProvider.GetRequiredService<ChecklistService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var records = await checklistService.GetAllAsync();

                var dueRecords = records
                    .Where(x =>
                        x.DueDate.HasValue &&
                        x.DueDate.Value.Date == DateTime.Today &&
                        string.Equals(x.Status?.Trim(), "Ongoing", StringComparison.OrdinalIgnoreCase) &&
                        x.DueReminderSent == false)
                    .ToList();

                foreach (var savedRecord in dueRecords)
                {
                    try
                    {
                        string? beforeImagePath = null;
                        string? afterImagePath = null;

                        if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
                        {
                            beforeImagePath = Path.Combine(
                                env.WebRootPath,
                                savedRecord.BeforeImagePath.TrimStart('/')
                                    .Replace("/", Path.DirectorySeparatorChar.ToString()));
                        }

                        if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
                        {
                            afterImagePath = Path.Combine(
                                env.WebRootPath,
                                savedRecord.AfterImagePath.TrimStart('/')
                                    .Replace("/", Path.DirectorySeparatorChar.ToString()));
                        }

                        string subject = $"MIL Audit Due Date Reminder - {DateTime.Now:dd-MM-yyyy}";
                        string body = BuildDueReminderBody(savedRecord);

                        await emailService.SendEmailWithInlineImagesAsync(
                            "Sharath_G@foxlink.com,Shilpa_M@foxlink.com,Sravani_M@foxlink.com",
                            subject,
                            body,
                            beforeImagePath,
                            afterImagePath);

                        await checklistService.MarkDueReminderSentAsync(savedRecord.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send due date reminder for record ID {RecordId}", savedRecord.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DueDateReminderService failed.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private string BuildDueReminderBody(dynamic savedRecord)
    {
        return $@"
        <html>
        <body style='font-family:Arial; background:#f7f7f7; padding:20px;'>

            <h2>MIL Audit Due Date Reminder</h2>

            <p>Hi Team,</p>

            <p>
                This is a reminder that the below audit point remains ongoing and has now reached its due date.
                Kindly prioritize this item and take the necessary actions at the earliest to ensure timely closure.
            </p>

            <div style='overflow-x:auto; width:100%;'>
                <table cellpadding='0' cellspacing='0'
                       style='border-collapse:collapse; width:100%; min-width:1500px; table-layout:fixed; font-size:12px; text-align:center; border:1px solid #d1d5db;'>

                    <thead>
                        <tr style='background:#d7e5e1; color:#111827;'>
                            <th style='width:60px; border:1px solid #c4cfd4; padding:10px;'>S.NO</th>
                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px;'>Date</th>
                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px;'>Week Code</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>Month</th>
                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px;'>Project</th>
                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px;'>Section</th>
                            <th style='width:80px; border:1px solid #c4cfd4; padding:10px;'>Line</th>
                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px;'>Station Name</th>
                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px;'>Problem Statement</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>Frequency</th>
                            <th style='width:110px; border:1px solid #c4cfd4; padding:10px;'>Issue Severity</th>
                            <th style='width:150px; border:1px solid #c4cfd4; padding:10px;'>Category</th>
                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px;'>Corrective Action</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>Due Date</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>CM DRI</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>Apple DRI</th>
                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px;'>Type of Audit</th>
                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px;'>Status</th>
                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px;'>Before Image</th>
                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px;'>After Image</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr style='background:#ffffff;'>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>1</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Date?.ToString("dd-MM-yyyy")}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.WeekCode}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Month}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Project}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Section}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Line}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.StationName}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px; text-align:left;'>{savedRecord.ProblemStatement}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Frequency}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.IssueSeverity}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Category}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px; text-align:left;'>{(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.CmDri}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.AppleDri}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.TypeOfAudit}</td>
                            <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>Ongoing</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                            <td style='border:1px solid #e5e7eb; padding:10px;'>AFTER_IMAGE_PLACEHOLDER</td>
                        </tr>
                    </tbody>
                </table>
            </div>

        </body>
        </html>";
    }
}