using E_MIL_Tracking_system.DTOs.Checklist;
using E_MIL_Tracking_system.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using E_MIL_Tracking_system.DTOs;

namespace E_MIL_Tracking_system.Controllers
{
    public class ChecklistController : Controller
    {
        private readonly ChecklistService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;

        public ChecklistController(
     ChecklistService service,
     IWebHostEnvironment env,
     IEmailService emailService,
     IBackgroundEmailQueue emailQueue,
     IConfiguration configuration)
        {
            _service = service;
            _env = env;
            _emailService = emailService;
            _emailQueue = emailQueue;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Checklist()
        {
            var records = await _service.GetAllAsync();
            return View(records);
        }

        [HttpGet]
        public async Task<IActionResult> ChecklistRecords(
            int? month,
            int? year,
            int? week,
            string? status,
            string? searchText)
        {
            var records = await _service.GetAllAsync();

            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedWeek = week ?? 0;

            var filteredRecords = records
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Month == selectedMonth &&
                            x.Date.Value.Year == selectedYear)
                .ToList();

            if (selectedWeek > 0)
            {
                var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);

                var weekStart = firstDayOfMonth.AddDays((selectedWeek - 1) * 7);
                var weekEnd = weekStart.AddDays(6);

                filteredRecords = filteredRecords
                    .Where(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date >= weekStart.Date &&
                        x.Date.Value.Date <= weekEnd.Date &&
                        x.Date.Value.Month == selectedMonth)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredRecords = filteredRecords
                    .Where(x => x.Status == status)
                    .ToList();
            }


            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.Trim().ToLower();

                filteredRecords = filteredRecords
                    .Where(x =>
                        (x.Section ?? "").ToLower().Contains(search) ||
                        (x.StationName ?? "").ToLower().Contains(search) ||
                        (x.IssueType ?? "").ToLower().Contains(search) ||
                        (x.ProblemStatement ?? "").ToLower().Contains(search) ||
                        (x.CmDri ?? "").ToLower().Contains(search) ||
                        (x.RespectiveDepartment ?? "").ToLower().Contains(search) ||
                        (x.AppleDri ?? "").ToLower().Contains(search) ||
                        (x.Auditor ?? "").ToLower().Contains(search))
                    .ToList();
            }

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedWeek = selectedWeek;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchText = searchText;

            return View(filteredRecords
                .OrderByDescending(x => x.Date)
                .ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Reports(
    int? month,
    int? year,
    int? week,
    string? status,
    string? searchText)
        {
            var records = await _service.GetAllAsync();

            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedWeek = week ?? 0;

            var filteredRecords = records
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Month == selectedMonth &&
                            x.Date.Value.Year == selectedYear &&
                            x.Date.Value.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            if (selectedWeek > 0)
            {
                var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);

                var workingDays = Enumerable.Range(0, DateTime.DaysInMonth(selectedYear, selectedMonth))
                    .Select(i => firstDayOfMonth.AddDays(i))
                    .Where(d => d.DayOfWeek != DayOfWeek.Sunday)
                    .ToList();

                var selectedWeekDates = workingDays
                    .Skip((selectedWeek - 1) * 6)
                    .Take(6)
                    .Select(d => d.Date)
                    .ToHashSet();

                filteredRecords = filteredRecords
                    .Where(x => x.Date.HasValue &&
                                selectedWeekDates.Contains(x.Date.Value.Date))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredRecords = filteredRecords
                    .Where(x => x.Status == status)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.Trim().ToLower();

                filteredRecords = filteredRecords
                    .Where(x =>
                        (x.Section ?? "").ToLower().Contains(search) ||
                        (x.StationName ?? "").ToLower().Contains(search) ||
                        (x.IssueType ?? "").ToLower().Contains(search) ||
                        (x.ProblemStatement ?? "").ToLower().Contains(search) ||
                        (x.CmDri ?? "").ToLower().Contains(search) ||
                        (x.RespectiveDepartment ?? "").ToLower().Contains(search) ||
                        (x.AppleDri ?? "").ToLower().Contains(search) ||
                        (x.Auditor ?? "").ToLower().Contains(search))
                    .ToList();
            }

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedWeek = selectedWeek;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchText = searchText;

            return View("~/Views/Reports/Reports.cshtml",
                filteredRecords.OrderByDescending(x => x.Date).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> A2Reports(
            int? month,
            int? year,
            int? week,
            string? status,
            string? searchText)
        {
            var records = await _service.GetAllAsync();

            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedWeek = week ?? 0;

            var filteredRecords = records
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Month == selectedMonth &&
                            x.Date.Value.Year == selectedYear)
                .ToList();

            if (selectedWeek > 0)
            {
                var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);

                var weekStart = firstDayOfMonth.AddDays((selectedWeek - 1) * 7);
                var weekEnd = weekStart.AddDays(6);

                filteredRecords = filteredRecords
                    .Where(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date >= weekStart.Date &&
                        x.Date.Value.Date <= weekEnd.Date &&
                        x.Date.Value.Month == selectedMonth)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredRecords = filteredRecords
                    .Where(x => x.Status == status)
                    .ToList();
            }


            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.Trim().ToLower();

                filteredRecords = filteredRecords
                    .Where(x =>
                        (x.Section ?? "").ToLower().Contains(search) ||
                        (x.StationName ?? "").ToLower().Contains(search) ||
                        (x.IssueType ?? "").ToLower().Contains(search) ||
                        (x.ProblemStatement ?? "").ToLower().Contains(search) ||
                        (x.CmDri ?? "").ToLower().Contains(search) ||
                        (x.RespectiveDepartment ?? "").ToLower().Contains(search) ||
                        (x.AppleDri ?? "").ToLower().Contains(search) ||
                        (x.Auditor ?? "").ToLower().Contains(search))
                    .ToList();
            }

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedWeek = selectedWeek;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchText = searchText;

            return View("~/Views/Checklist/A2Reports.cshtml",
                filteredRecords.OrderByDescending(x => x.Date).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateChecklistDto dto)
        {
            dto.Status = dto.BeforeImages != null && dto.BeforeImages.Any(x => x.Length > 0)
            ? "Ongoing"
            : "";

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide all the input fields";
                var records = await _service.GetAllAsync();
                return View("Checklist", records);
            }

            int savedId = await _service.SaveAsync(dto, _env.WebRootPath);

            var savedRecord = await _service.GetByIdAsync(savedId);

            if (savedRecord == null)
            {
                TempData["ErrorMessage"] = "Checklist saved, but mail data not found.";
                return RedirectToAction(nameof(Checklist));
            }

            TempData["SuccessMessage"] = "Checklist form data submitted successfully";
            return RedirectToAction(nameof(Checklist));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateChecklistRecord(UpdateChecklistRecordDto dto)
        {
            if (dto.Id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid checklist record";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            await _service.UpdateChecklistRecordAsync(dto);

            TempData["SuccessMessage"] = "RCCA updated successfully.";
            return RedirectToAction(nameof(ChecklistRecords));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSelectedChecklistMail([FromBody] SendSelectedChecklistMailDto dto)
        {
            if (dto == null || dto.Ids == null || !dto.Ids.Any())
            {
                return Json(new { success = false, message = "Please select at least one record." });
            }

            var records = new List<ChecklistResponseDto>();

            foreach (var id in dto.Ids.Distinct())
            {
                var record = await _service.GetByIdAsync(id);

                if (record != null)
                {
                    records.Add(record);
                }
            }

            if (!records.Any())
            {
                return Json(new { success = false, message = "Selected records not found." });
            }

            if (dto.Type == "RCCA")
            {
                await SendSelectedRccaMailAsync(records);

                return Json(new
                {
                    success = true,
                    message = "Selected RCCA records mail sent successfully."
                });
            }

            if (dto.Type == "AFTER_IMAGE")
            {
                await SendSelectedAfterImageMailAsync(records);

                return Json(new
                {
                    success = true,
                    message = "Selected After Image records mail sent successfully."
                });
            }

            return Json(new { success = false, message = "Invalid mail type." });
        }

        private async Task SendSelectedRccaMailAsync(List<ChecklistResponseDto> records)
        {
            var recipients = new List<string>
    {
        "Sharath_G@foxlink.com",
        "Charitha_J@foxlink.com"
    };

            foreach (var record in records)
            {
                await _service.ResetApprovalsAsync(record.Id, recipients);
            }

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string subject = $"E-MIL Audit RCCA Updated {DateTime.Now:dd-MM-yyyy}";

            foreach (var recipient in recipients)
            {
                var inlineImages = new List<InlineEmailImage>();
                int serialNo = 1;
                string rowsHtml = "";

                foreach (var record in records)
                {
                    string encodedEmail = Uri.EscapeDataString(recipient);

                    string approveUrl = $"{baseUrl}/Checklist/ApproveFromEmail?id={record.Id}&email={encodedEmail}&approvalType=RCCA";
                    string rejectUrl = $"{baseUrl}/Checklist/RejectFromEmail?id={record.Id}&email={encodedEmail}&approvalType=RCCA";

                    string beforeImageHtml = BuildInlineImages(record.BeforeImagePath, inlineImages, "before");

                    rowsHtml += $@"
            <tr> 
                <td style='border:1px solid #e5e7eb; padding:10px;'>{serialNo}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Section}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.StationName}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueType}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; max-width:260px; word-wrap:break-word; white-space:normal; line-height:1.6;'>{record.ProblemStatement}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Frequency}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueSeverity}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Category}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; font-weight:600;'>{(string.IsNullOrWhiteSpace(record.Rcca) ? "-" : record.Rcca)}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Date?.ToString("dd-MM-yyyy")}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.DueDate?.ToString("dd-MM-yyyy")}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.CmDri}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.AppleDri}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.TypeOfAudit}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{beforeImageHtml}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>{record.Status}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>
                    <a href='{approveUrl}'
                       style='display:inline-block; padding:10px 16px; background:#22c55e; color:#ffffff; text-decoration:none; font-weight:bold; border-radius:8px;'>
                        ACCEPT
                    </a>
                </td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>
                    <a href='{rejectUrl}'
                       style='display:inline-block; padding:10px 16px; background:#ef4444; color:#ffffff; text-decoration:none; font-weight:bold; border-radius:8px;'>
                        REJECT
                    </a>
                </td>
            </tr>";

                    serialNo++;
                }

                string body = $@"
        <html>
        <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>
            <div style='background:#ffffff; padding:20px; border-radius:12px;'>

                <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                    E-MIL Audit RCCA Updated
                </h2>

                <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>
                <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                    RCCA has been updated for the below selected MIL audit points.
                    Please review the corrective action details and take action.
                </p>

                <table cellpadding='0' cellspacing='0'
                       style='border-collapse:collapse; width:100%; min-width:1900px; font-size:12px; text-align:center;'>
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
                            <th style='border:1px solid #c4cfd4; padding:10px;'>Status</th>
                            <th style='border:1px solid #c4cfd4; padding:10px;'>Accept</th>
                            <th style='border:1px solid #c4cfd4; padding:10px;'>Reject</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rowsHtml}
                    </tbody>
                </table>

                <p style='margin-top:20px; font-size:13px; color:#6b7280;'>
                    This response is tracked for: {recipient}
                </p>
            </div>
        </body>
        </html>";

                _emailQueue.QueueEmail(async cancellationToken =>
                {
                    await _emailService.SendEmailWithMultipleInlineImagesAsync(
                        recipient,
                        subject,
                        body,
                        inlineImages
                    );
                });
            }
        }


        private async Task SendSelectedAfterImageMailAsync(List<ChecklistResponseDto> records)
        {
            var recipients = new List<string>
            {
                "Sharath_G@foxlink.com"
            };

            foreach (var record in records)
            {
                await _service.ResetApprovalsAsync(record.Id, recipients);
            }

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string subject = $"MIL Audit Review Required - {DateTime.Now:dd-MM-yyyy}";

            foreach (var recipient in recipients)
            {
                var inlineImages = new List<InlineEmailImage>();
                int serialNo = 1;
                string rowsHtml = "";

                foreach (var record in records)
                {
                    string encodedEmail = Uri.EscapeDataString(recipient);

                    string approveUrl = $"{baseUrl}/Checklist/ApproveFromEmail?id={record.Id}&email={encodedEmail}&approvalType=AFTER_IMAGE";
                    string rejectUrl = $"{baseUrl}/Checklist/RejectFromEmail?id={record.Id}&email={encodedEmail}&approvalType=AFTER_IMAGE";

                    string beforeImageHtml = BuildInlineImages(record.BeforeImagePath, inlineImages, "before");
                    string afterImageHtml = BuildInlineImages(record.AfterImagePath, inlineImages, "after");

                    rowsHtml += $@"
            <tr>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{serialNo}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Section}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.StationName}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueType}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; max-width:260px; word-wrap:break-word; white-space:normal; line-height:1.6;'>{record.ProblemStatement}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Frequency}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueSeverity}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Category}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; font-weight:600;'>{(string.IsNullOrWhiteSpace(record.Rcca) ? "-" : record.Rcca)}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Date?.ToString("dd-MM-yyyy")}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.DueDate?.ToString("dd-MM-yyyy")}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.CmDri}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.AppleDri}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{record.TypeOfAudit}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{beforeImageHtml}</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>{afterImageHtml}</td>
                <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>Ongoing</td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>
                    <a href='{approveUrl}'
                       style='display:inline-block; padding:10px 16px; background:#22c55e; color:#ffffff; text-decoration:none; font-weight:bold; border-radius:8px;'>
                        ACCEPT
                    </a>
                </td>
                <td style='border:1px solid #e5e7eb; padding:10px;'>
                    <a href='{rejectUrl}'
                       style='display:inline-block; padding:10px 16px; background:#ef4444; color:#ffffff; text-decoration:none; font-weight:bold; border-radius:8px;'>
                        REJECT
                    </a>
                </td>
            </tr>";

                    serialNo++;
                }

                string body = $@"
        <html>
        <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>
            <div style='background:#ffffff; padding:20px; border-radius:12px;'>
                <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                    MIL Audit Review Required
                </h2>

                <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>
                <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                    After Image has been uploaded successfully for the below selected audit points.
                    Please review and take action.
                </p>

                <table cellpadding='0' cellspacing='0'
                       style='border-collapse:collapse; width:100%; min-width:1900px; font-size:12px; text-align:center;'>
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
                            <th style='border:1px solid #c4cfd4; padding:10px;'>Accept</th>
                            <th style='border:1px solid #c4cfd4; padding:10px;'>Reject</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rowsHtml}
                    </tbody>
                </table>

                <p style='margin-top:20px; font-size:13px; color:#6b7280;'>
                    This response is tracked for: {recipient}
                </p>
            </div>
        </body>
        </html>";

                _emailQueue.QueueEmail(async cancellationToken =>
                {
                    await _emailService.SendEmailWithMultipleInlineImagesAsync(
                        recipient,
                        subject,
                        body,
                        inlineImages
                    );
                });
            }
        }


        private string BuildInlineImages(
    string imagePaths,
    List<InlineEmailImage> inlineImages,
    string prefix)
        {
            if (string.IsNullOrWhiteSpace(imagePaths))
            {
                return "-";
            }

            string imageHtml = "";

            foreach (var imagePath in imagePaths.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.Combine(
                    _env.WebRootPath,
                    imagePath.Trim().TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(fullPath))
                {
                    string contentId = prefix + "_" + Guid.NewGuid().ToString("N");

                    inlineImages.Add(new InlineEmailImage
                    {
                        ContentId = contentId,
                        FilePath = fullPath
                    });

                    imageHtml += $@"
            <div style='display:inline-block; margin:4px;'>
                <img src='cid:{contentId}'
                     width='80'
                     height='60'
                     style='object-fit:cover;border-radius:6px;border:1px solid #d1d5db;' />
            </div>";
                }
            }

            return string.IsNullOrWhiteSpace(imageHtml) ? "-" : imageHtml;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAfterImage(int id, IFormFile afterImage)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid checklist record.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            var recordBeforeUpload = await _service.GetByIdAsync(id);

            if (recordBeforeUpload == null)
            {
                TempData["ErrorMessage"] = "Checklist record not found.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            if (string.IsNullOrWhiteSpace(recordBeforeUpload.Rcca))
            {
                TempData["ErrorMessage"] = "Please enter RCCA before uploading After Image.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            if (afterImage == null || afterImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select or capture an After Image.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            await _service.UploadAfterImageAsync(id, afterImage, _env.WebRootPath);

            await _service.UpdateStatusAsync(id, "Ongoing");

            TempData["SuccessMessage"] = "After Image uploaded successfully.";
            return RedirectToAction(nameof(ChecklistRecords));
        }

        private async Task SendAcceptRejectMailAsync(
        ChecklistResponseDto record,
        string actionByEmail,
        string actionStatus,
        string actionMessage)
        {
            string subject = $"MIL Audit {actionStatus} - {DateTime.Now:dd-MM-yyyy}";

            string recipients =
                "Sharath_G@foxlink.com,Charitha_J@foxlink.com";
            // "Sharath_G@foxlink.com,Ipqc1_Tpt@foxlink.com,Maintenence_Tpt@foxlink.com";

            string? beforeImagePath = null;
            string? afterImagePath = null;

            if (!string.IsNullOrWhiteSpace(record.BeforeImagePath))
            {
                var firstBeforeImage = record.BeforeImagePath
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(firstBeforeImage))
                {
                    beforeImagePath = Path.Combine(
                        _env.WebRootPath,
                        firstBeforeImage.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(record.AfterImagePath))
            {
                afterImagePath = Path.Combine(
                    _env.WebRootPath,
                    record.AfterImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            string body = $@"
                <html>
                <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>
                    <div style='background:#ffffff; padding:20px; border-radius:12px;'>

                        <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                            MIL Audit {actionStatus}
                        </h2>

                        <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>
                        <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                        <p style='margin:0 0 12px; font-size:14px; line-height:1.6; font-weight:600;'>
                            {actionMessage}
                        </p>

                        <p style='margin:0 0 20px; font-size:13px; color:#374151;'>
                            Action taken by: <b>{actionByEmail}</b>
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
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Section}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.StationName}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueType}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; line-height:1.6;'>{record.ProblemStatement}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Frequency}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.IssueSeverity}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Category}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left; font-weight:600;'>{record.Rcca}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.Date?.ToString("dd-MM-yyyy")}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.DueDate?.ToString("dd-MM-yyyy")}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.CmDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.AppleDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{record.TypeOfAudit}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>AFTER_IMAGE_PLACEHOLDER</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>{record.Status}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailWithInlineImagesAsync1(
                recipients,
                subject,
                body,
                beforeImagePath,
                afterImagePath
            );
        }

        [HttpGet]
        public async Task<IActionResult> ApproveFromEmail(int id, string email, string? approvalType)
        {
            try
            {
                if (id <= 0 || string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Invalid approval link.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                email = Uri.UnescapeDataString(email).Trim();
                approvalType = approvalType?.Trim().ToUpper();
                var currentStatus = await _service.GetApprovalStatusAsync(id, email);

                if (currentStatus == "Accepted")
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Already Accepted.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                if (currentStatus == "Rejected")
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Already Rejected. You cannot accept now.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                await _service.MarkAcceptedAsync(id, email);

                bool allAccepted = await _service.AreAllAcceptedAsync(id);

                if (approvalType == "RCCA")
                {
                    await _service.UpdateRccaAcceptedAsync(id, true);
                    await _service.UpdateStatusAsync(id, "Ongoing");
                }
                else
                {
                    if (allAccepted)
                    {
                        await _service.UpdateStatusAsync(id, "Closed");
                    }
                    else
                    {
                        await _service.UpdateStatusAsync(id, "Ongoing");
                    }
                }

                var record = await _service.GetByIdAsync(id);

                if (record != null)
                {
                    record.Status = allAccepted ? "Closed" : "Ongoing";

                    try
                    {
                        string acceptMailMessage = approvalType == "RCCA"
                            ? "The RCCA details have been reviewed and accepted. The corrective action explanation is verified and approved. Please proceed with uploading the After Image evidence."
                            : "The actions were cross-verified on the line, no issues were found, and supporting images have been uploaded as evidence.";

                        await SendAcceptRejectMailAsync(
                            record,
                            email,
                            approvalType == "RCCA" ? "RCCA ACCEPTED" : "ACCEPTED",
                            acceptMailMessage
                        );
                    }
                    catch (Exception mailEx)
                    {
                        
                    }
                }

                ViewBag.IsSuccess = true;
                ViewBag.Message = approvalType == "RCCA"
                    ? "RCCA Approved Successfully."
                    : "After Image Approved Successfully.";

                return View("~/Views/Checklist/ApprovalResult.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = ex.Message;
                return View("~/Views/Checklist/ApprovalResult.cshtml");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RejectFromEmail(int id, string email, string approvalType)
        {
            try
            {
                if (id <= 0 || string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Invalid rejection link.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                email = Uri.UnescapeDataString(email).Trim();
                approvalType = approvalType?.Trim().ToUpper();

                var currentStatus = await _service.GetApprovalStatusAsync(id, email);

                if (currentStatus == "Rejected")
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Already Rejected.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                if (currentStatus == "Accepted")
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Already Accepted. You cannot reject now.";
                    return View("~/Views/Checklist/ApprovalResult.cshtml");
                }

                await _service.MarkRejectedAsync(id, email);
                if (approvalType == "RCCA")
                {
                    await _service.UpdateRccaAcceptedAsync(id, false);
                }
                await _service.UpdateStatusAsync(id, "Ongoing");

                var record = await _service.GetByIdAsync(id);

                if (record != null)
                {
                    record.Status = "Ongoing";

                    try
                    {
                        string rejectMailMessage = approvalType == "RCCA"
                            ? "The RCCA details have been reviewed and rejected. The corrective action explanation is not acceptable. Please update the RCCA and resubmit it for approval."
                            : "The actions were cross-verified on the line, we have found an issue. For details, refer below.";

                        await SendAcceptRejectMailAsync(
                            record,
                            email,
                            approvalType == "RCCA" ? "RCCA REJECTED" : "REJECTED",
                            rejectMailMessage
                        );
                    }
                    catch (Exception mailEx)
                    {
                        
                    }
                }

                ViewBag.IsSuccess = false;
                ViewBag.Message = approvalType == "RCCA"
                    ? "RCCA Rejected Successfully."
                    : "After Image Rejected Successfully.";

                return View("~/Views/Checklist/ApprovalResult.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = ex.Message;
                return View("~/Views/Checklist/ApprovalResult.cshtml");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "You do not have permission to update status.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            if (id <= 0 || string.IsNullOrWhiteSpace(status))
            {
                TempData["ErrorMessage"] = "Invalid status update request.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            await _service.UpdateStatusAsync(id, status);
            TempData["SuccessMessage"] = "Status updated successfully.";
            return RedirectToAction(nameof(ChecklistRecords));
        }

        public async Task<IActionResult> AuditHours()
        {
            var checklistRecords = await _service.GetAllAsync();

            var auditHours = new List<AuditHourDto>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT AuditDate, AuditHour FROM AuditHours";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            auditHours.Add(new AuditHourDto
                            {
                                AuditDate = reader["AuditDate"]?.ToString(),
                                AuditHour = Convert.ToDecimal(reader["AuditHour"])
                            });
                        }
                    }
                }
            }

            var model = new AuditHoursPageDto
            {
                ChecklistRecords = checklistRecords,
                AuditHours = auditHours
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAuditHour(
        string AuditDateText,
        decimal AuditHour,
        string? ReturnUrl)
        {
            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"INSERT INTO AuditHours (AuditDate, AuditHour)
                         VALUES (@AuditDate, @AuditHour)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AuditDate", AuditDateText);
                    cmd.Parameters.AddWithValue("@AuditHour", AuditHour);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            TempData["SuccessMessage"] = "Audit hour saved successfully.";

            if (!string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTodayChecklistMail()
        {
            var records = await _service.GetAllAsync();

            var todayRecords = records
                .Where(x => x.Date.HasValue && x.Date.Value.Date == DateTime.Today)
                .OrderByDescending(x => x.Date)
                .ToList();

            if (!todayRecords.Any())
            {
                TempData["ErrorMessage"] = "No records found for today";
                return RedirectToAction(nameof(Checklist));
            }

            var inlineImages = new List<InlineEmailImage>();
            string rows = "";
            int serialNo = 1;

            foreach (var item in todayRecords)
            {
                string beforeImageHtml = "-";

                string problemStatement = string.IsNullOrWhiteSpace(item.ProblemStatement)
                ? "-"
                : string.Join("<br/>",
                    Enumerable.Range(0, (int)Math.Ceiling(item.ProblemStatement.Length / 50.0))
                    .Select(i => item.ProblemStatement.Substring(
                        i * 50,
                        Math.Min(50, item.ProblemStatement.Length - (i * 50))
                    )));


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

                        if (System.IO.File.Exists(fullPath))
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
                                         style='display:block;
                                                object-fit:cover;
                                                border-radius:6px;
                                                border:1px solid #d1d5db;' />
                                </div>";
                            }
                        }

                        if (string.IsNullOrWhiteSpace(beforeImageHtml))
                        {
                            beforeImageHtml = "-";
                        }
                    }

                    rows += $@"
                    <tr>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{serialNo}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Section}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.StationName}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.IssueType}</td>
                    <td style='border:1px solid #e5e7eb;
                               padding:10px;
                               text-align:left;
                               max-width:260px;
                               word-wrap:break-word;
                               white-space:normal;
                               line-height:1.6;'>
                        {problemStatement}
                    </td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Frequency}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.IssueSeverity}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Category}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Date?.ToString("dd-MM-yyyy")}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.DueDate?.ToString("dd-MM-yyyy")}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.CmDri}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.RespectiveDepartment}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.AppleDri}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.TypeOfAudit}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px;'>{item.Auditor}</td>
                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:center; vertical-align:middle; white-space:normal;'>
                        {beforeImageHtml}
                    </td>
                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>{item.Status}</td>
                </tr>";

                serialNo++;
            }

            string subject = $"MIL Audit Report - {DateTime.Today:dd-MM-yyyy}";

            string body = $@"
            <html>
            <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>
                <div style='background:#ffffff; padding:20px; border-radius:12px;'>
                    <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                        MIL Audit Report
                    </h2>

                    <p style='margin:0 0 10px; font-size:14px;'>
                        Hi Team,
                    </p>

                    <p style='margin:0 0 10px; font-size:14px;'>
                        Good day.
                    </p>

                    <p style='margin:0 0 10px; font-size:14px; line-height:1.6;'>
                        I have attached the MIL audit points for your review.
                    </p>

                    <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                        Please take necessary actions to address the identified issues and provide an update on the CP date,
                        root cause analysis and corrective actions for the mentioned points.
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
                                <th style='border:1px solid #c4cfd4; padding:10px;'>Status</th>
                            </tr>
                        </thead>
                        <tbody>{rows}</tbody>
                    </table>
                </div>
            </body>
            </html>";

            _emailQueue.QueueEmail(async cancellationToken =>
            {
                await _emailService.SendEmailWithMultipleInlineImagesAsync(
                    "Sharath_G@foxlink.com,Charitha_J@foxlink.com",
                    subject,
                    body,
                    inlineImages
                );
            });

            TempData["SuccessMessage"] = "Today records sent successfully";
            return RedirectToAction(nameof(Checklist));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateChecklistMainRecord(CreateChecklistDto dto)
        {
            if (dto.Id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid checklist record.";
                return RedirectToAction(nameof(Checklist));
            }

            ModelState.Remove("BeforeImages");
            ModelState.Remove("ExistingBeforeImages");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide all the input fields";

                var records = await _service.GetAllAsync();
                return View("Checklist", records);
            }

            await _service.UpdateMainChecklistAsync(dto, _env.WebRootPath);

            TempData["SuccessMessage"] = "Checklist updated successfully.";

            return RedirectToAction(nameof(Checklist));
        }
    }
}