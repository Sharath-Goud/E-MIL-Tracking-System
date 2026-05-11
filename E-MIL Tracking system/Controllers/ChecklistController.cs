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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreateChecklistDto dto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        TempData["ErrorMessage"] = "Please provide all the input fields";
        //        var records = await _service.GetAllAsync();
        //        return View("Checklist", records);
        //    }

        //    int savedId = await _service.SaveAsync(dto, _env.WebRootPath);

        //    var savedRecord = await _service.GetByIdAsync(savedId);

        //    if (savedRecord == null)
        //    {
        //        TempData["ErrorMessage"] = "Checklist saved, but mail data not found.";
        //        return RedirectToAction(nameof(Checklist));
        //    }

        //    //string subject = $"MIL Audit report {DateTime.Now:dd-MM-yyyy}";

        //    //string body = $@"
        //    //    <!DOCTYPE html>
        //    //    <html>
        //    //    <head>
        //    //        <meta charset='UTF-8'>
        //    //    </head>
        //    //    <body style='margin:0; padding:0; background:#f4f6f8; font-family:Arial, sans-serif; color:#1f2937;'>

        //    //        <div style='max-width:1200px; margin:0 auto; padding:24px;'>

        //    //            <div style='background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; padding:22px;'>

        //    //                <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
        //    //                    MIL Audit Report
        //    //                </h2>

        //    //                <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>

        //    //                <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

        //    //                <p style='margin:0 0 10px; font-size:14px; line-height:1.6;'>
        //    //                    I have attached the MIL audit points for your review.
        //    //                </p>

        //    //                <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
        //    //                    Please take necessary actions to address the identified issues and provide an update on the CP date,
        //    //                    root cause analysis and corrective actions for the mentioned points.
        //    //                </p>

        //    //                <div style='overflow-x:auto; width:100%;'>
        //    //                     <table cellpadding='0' cellspacing='0'
        //    //                        style='border-collapse:collapse; width:100%; min-width:1500px; table-layout:fixed; font-family:Arial, sans-serif; font-size:12px; text-align:center; border:1px solid #d1d5db;'>

        //    //                         <thead>
        //    //                            <tr style='background:#d7e5e1; color:#111827;'>
        //    //                                <th style='width:60px; border:1px solid #c4cfd4; padding:10px 8px;'>S.NO</th>
        //    //                                <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Date</th>
        //    //                                <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Week<br/>Code</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Month</th>
        //    //                                <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Project</th>
        //    //                                <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Section</th>
        //    //                                <th style='width:80px; border:1px solid #c4cfd4; padding:10px 8px;'>Line</th>
        //    //                                <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Station Name</th>
        //    //                                <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Problem Statement</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Frequency</th>
        //    //                                <th style='width:110px; border:1px solid #c4cfd4; padding:10px 8px;'>Issue<br/>Severity</th>
        //    //                                <th style='width:150px; border:1px solid #c4cfd4; padding:10px 8px;'>Category</th>
        //    //                                <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Corrective Action</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Due Date</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>CM DRI</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Apple DRI</th>
        //    //                                <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Type of Audit</th>
        //    //                                <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Status</th>
        //    //                                <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Before Image</th>
        //    //                                <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>After Image</th>
        //    //                            </tr>
        //    //                        </thead>


        //    //                        <tbody>
        //    //                            <tr style='background:#ffffff; color:#1f2937;'>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>1</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Date?.ToString("dd-MM-yyyy")}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.WeekCode}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Month}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.Project}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Section}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Line}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.StationName}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5;'>{savedRecord.ProblemStatement}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Frequency}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.IssueSeverity}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Category}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5; font-weight:600;'>{(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.CmDri}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.AppleDri}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.TypeOfAudit}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:700; color:#b45309;'>{savedRecord.Status}</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>BEFORE_IMAGE_PLACEHOLDER</td>
        //    //                                <td style='border:1px solid #e5e7eb; padding:10px 8px;'>AFTER_IMAGE_PLACEHOLDER</td>
        //    //                            </tr>
        //    //                        </tbody>
        //    //                    </table>
        //    //                </div>
        //    //            </div>
        //    //        </div>

        //    //    </body>
        //    //    </html>";

        //    //string? beforeImageFullPath = null;
        //    //string? afterImageFullPath = null;

        //    //if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
        //    //{
        //    //    beforeImageFullPath = Path.Combine(
        //    //        _env.WebRootPath,
        //    //        savedRecord.BeforeImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
        //    //    );
        //    //}

        //    //if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
        //    //{
        //    //    afterImageFullPath = Path.Combine(
        //    //        _env.WebRootPath,
        //    //        savedRecord.AfterImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
        //    //    );
        //    //}

        //    ////Ipqc1_Tpt@foxlink.com, Maintenence_Tpt@foxlink.com, Aravindhan_S@foxlink.com, Balaji_K@foxlink.com, Production_Tpt@foxlink.com
        //    //// Thanuja_C@foxlink.com, Jeevankumar_V@foxlink.com, Harish_K@foxlink.com, Rokeshkumar_D@foxlink.com
        //    //// Satheeshkumar_R@foxlink.com, Poojith_S@foxlink.com, Vinodh_S@foxlink.com, Ambethkar_M@foxlink.com
        //    //_emailQueue.QueueEmail(async cancellationToken =>
        //    //{
        //    //    await _emailService.SendEmailWithInlineImagesAsync(
        //    //        "Sharath_G@foxlink.com",
        //    //        subject,
        //    //        body,
        //    //        beforeImageFullPath,
        //    //        afterImageFullPath
        //    //    );
        //    //});

        //    TempData["SuccessMessage"] = "Checklist form data submitted successfully";
        //    return RedirectToAction(nameof(Checklist));
        //}


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

            var savedRecord = await _service.GetByIdAsync(dto.Id);

            if (savedRecord == null)
            {
                TempData["ErrorMessage"] = "RCCA updated, but mail data not found.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            string subject = $"MIL Audit RCCA Updated {DateTime.Now:dd-MM-yyyy}";

            string body = $@"
            <html>
            <body style='font-family:Arial, sans-serif; background:#f4f6f8; padding:20px;'>

                <div style='background:#ffffff; padding:20px; border-radius:12px;'>

                    <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                        MIL Audit RCCA Updated
                    </h2>

                    <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>

                    <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                    <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                        RCCA has been updated for the below MIL audit point. Please review the corrective action details.
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
                                <td style='border:1px solid #e5e7eb; padding:10px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                                <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>{savedRecord.Status}</td>
                            </tr>
                        </tbody>
                    </table>

                </div>
            </body>
            </html>";

            string? beforeImageFullPath = null;

            if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
            {
                beforeImageFullPath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.BeforeImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            //Ipqc1_Tpt@foxlink.com, Maintenence_Tpt@foxlink.com, Aravindhan_S@foxlink.com, Balaji_K@foxlink.com, Production_Tpt@foxlink.com
            // Thanuja_C@foxlink.com, Jeevankumar_V@foxlink.com, Harish_K@foxlink.com, Rokeshkumar_D@foxlink.com
            // Satheeshkumar_R@foxlink.com, Poojith_S@foxlink.com, Vinodh_S@foxlink.com, Ambethkar_M@foxlink.com
            _emailQueue.QueueEmail(async cancellationToken =>
            {
                await _emailService.SendEmailWithInlineImagesAsync(
                    "Sharath_G@foxlink.com",
                    subject,
                    body,
                    beforeImageFullPath
                );
            });

            TempData["SuccessMessage"] = "RCCA updated successfully.";
            return RedirectToAction(nameof(ChecklistRecords));
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

            var savedRecord = await _service.GetByIdAsync(id);

            if (savedRecord == null)
            {
                TempData["ErrorMessage"] = "Record not found after image upload.";
                return RedirectToAction(nameof(ChecklistRecords));
            }
            //Ipqc1_Tpt@foxlink.com, Maintenence_Tpt@foxlink.com, Aravindhan_S@foxlink.com, Balaji_K@foxlink.com, Production_Tpt@foxlink.com
            // Thanuja_C@foxlink.com, Jeevankumar_V@foxlink.com, Harish_K@foxlink.com, Rokeshkumar_D@foxlink.com
            // Satheeshkumar_R@foxlink.com, Poojith_S@foxlink.com, Vinodh_S@foxlink.com, Ambethkar_M@foxlink.com
            var recipients = new List<string>
            {
                "Sharath_G@foxlink.com"
            };

            await _service.ResetApprovalsAsync(id, recipients);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string subject = $"MIL Audit Review Required - {DateTime.Now:dd-MM-yyyy}";

            string? beforeImagePath = null;
            string? afterImagePath = null;

            if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
            {
                var firstBeforeImage = savedRecord.BeforeImagePath
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

            if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
            {
                afterImagePath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.AfterImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            foreach (var recipient in recipients)
            {
                string encodedEmail = Uri.EscapeDataString(recipient);

                string approveUrl = $"{baseUrl}/Checklist/ApproveFromEmail?id={id}&email={encodedEmail}";
                string rejectUrl = $"{baseUrl}/Checklist/RejectFromEmail?id={id}&email={encodedEmail}";

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
                            After Image has been uploaded successfully for the below audit point.
                            Please review and take action.
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
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>AFTER_IMAGE_PLACEHOLDER</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>Ongoing</td>
                                </tr>
                            </tbody>
                        </table>

                        <table role='presentation' style='margin-top:24px;'>
                            <tr>
                                <td style='background:#22c55e; border-radius:8px;'>
                                    <a href='{approveUrl}'
                                       style='display:inline-block; padding:14px 26px; color:#ffffff; text-decoration:none; font-weight:bold;'>
                                        ACCEPT
                                    </a>
                                </td>

                                <td style='width:16px;'></td>

                                <td style='background:#ef4444; border-radius:8px;'>
                                    <a href='{rejectUrl}'
                                       style='display:inline-block; padding:14px 26px; color:#ffffff; text-decoration:none; font-weight:bold;'>
                                        REJECT
                                    </a>
                                </td>
                            </tr>
                        </table>

                        <p style='margin-top:20px; font-size:13px; color:#6b7280;'>
                            This response is tracked for: {recipient}
                        </p>

                    </div>
                </body>
                </html>";

                try
                {
                    await _emailService.SendEmailWithInlineImagesAsync1(
                        recipient,
                        subject,
                        body,
                        beforeImagePath,
                        afterImagePath
                    );
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "After image uploaded, but mail failed: " + ex.Message;
                    return RedirectToAction(nameof(ChecklistRecords));
                }
            }

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
                "Sharath_G@foxlink.com";
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
        public async Task<IActionResult> ApproveFromEmail(int id, string email)
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

                await _service.MarkAcceptedAsync(id, email);

                bool allAccepted = await _service.AreAllAcceptedAsync(id);

                if (allAccepted)
                {
                    await _service.UpdateStatusAsync(id, "Closed");
                }
                else
                {
                    await _service.UpdateStatusAsync(id, "Ongoing");
                }

                var record = await _service.GetByIdAsync(id);

                if (record != null)
                {
                    record.Status = allAccepted ? "Closed" : "Ongoing";

                    try
                    {
                        await SendAcceptRejectMailAsync(
                            record,
                            email,
                            "ACCEPTED",
                            "The actions were cross-verified on the line, no issues were found, and supporting images have been uploaded as evidence"
                        );
                    }
                    catch (Exception mailEx)
                    {
                        ViewBag.IsSuccess = true;
                        ViewBag.Message = "Approved successfully, but mail failed: " + mailEx.Message;
                        return View("~/Views/Checklist/ApprovalResult.cshtml");
                    }
                }

                ViewBag.IsSuccess = true;
                ViewBag.Message = allAccepted
                    ? "The audit checklist has been successfully reviewed and approved. All verification activities were completed successfully."
                    : "The audit checklist has been successfully reviewed. Thank you for your response and support.";

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
        public async Task<IActionResult> RejectFromEmail(int id, string email)
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

                await _service.MarkRejectedAsync(id, email);
                await _service.UpdateStatusAsync(id, "Ongoing");

                var record = await _service.GetByIdAsync(id);

                if (record != null)
                {
                    record.Status = "Ongoing";

                    try
                    {
                        await SendAcceptRejectMailAsync(
                            record,
                            email,
                            "REJECTED",
                            "The actions were cross-verified on the line, we have found an issues, for details refer below"
                        );
                    }
                    catch (Exception mailEx)
                    {
                        ViewBag.IsSuccess = false;
                        ViewBag.Message = "Rejected successfully, but mail failed: " + mailEx.Message;
                        return View("~/Views/Checklist/ApprovalResult.cshtml");
                    }
                }

                ViewBag.IsSuccess = false;
                ViewBag.Message =
                    "The audit checklist verification was not approved during cross-verification. Additional corrective actions and validation are required before final closure.";

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
                    "Sharath_G@foxlink.com",
                    subject,
                    body,
                    inlineImages
                );
            });

            TempData["SuccessMessage"] = "Today records sent successfully";
            return RedirectToAction(nameof(Checklist));
        }
    }
}