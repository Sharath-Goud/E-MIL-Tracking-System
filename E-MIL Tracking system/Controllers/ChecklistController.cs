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
        public async Task<IActionResult> ChecklistRecords()
        {
            var records = await _service.GetAllAsync();
            return View(records);
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var records = await _service.GetAllAsync();
            return View("~/Views/Reports/Reports.cshtml", records);
        }

        [HttpGet]
        public async Task<IActionResult> A2Reports()
        {
            var records = await _service.GetAllAsync();
            return View("~/Views/Checklist/A2Reports.cshtml",records);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateChecklistDto dto)
        {
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

            string subject = $"MIL Audit report {DateTime.Now:dd-MM-yyyy}";

            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                </head>
                <body style='margin:0; padding:0; background:#f4f6f8; font-family:Arial, sans-serif; color:#1f2937;'>

                    <div style='max-width:1200px; margin:0 auto; padding:24px;'>

                        <div style='background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; padding:22px;'>

                            <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                                MIL Audit Report
                            </h2>

                            <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>

                            <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                            <p style='margin:0 0 10px; font-size:14px; line-height:1.6;'>
                                I have attached the MIL audit points for your review.
                            </p>

                            <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                                Please take necessary actions to address the identified issues and provide an update on the CP date,
                                root cause analysis and corrective actions for the mentioned points.
                            </p>

                            <div style='overflow-x:auto; width:100%;'>
                                 <table cellpadding='0' cellspacing='0'
                                    style='border-collapse:collapse; width:100%; min-width:1500px; table-layout:fixed; font-family:Arial, sans-serif; font-size:12px; text-align:center; border:1px solid #d1d5db;'>

                                     <thead>
                                        <tr style='background:#d7e5e1; color:#111827;'>
                                            <th style='width:60px; border:1px solid #c4cfd4; padding:10px 8px;'>S.NO</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Date</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Week<br/>Code</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Month</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Project</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Section</th>
                                            <th style='width:80px; border:1px solid #c4cfd4; padding:10px 8px;'>Line</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Station Name</th>
                                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Problem Statement</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Frequency</th>
                                            <th style='width:110px; border:1px solid #c4cfd4; padding:10px 8px;'>Issue<br/>Severity</th>
                                            <th style='width:150px; border:1px solid #c4cfd4; padding:10px 8px;'>Category</th>
                                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Corrective Action</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Due Date</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>CM DRI</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Apple DRI</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Type of Audit</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Status</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Before Image</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>After Image</th>
                                        </tr>
                                    </thead>

                                    <tbody>
                                        <tr style='background:#ffffff; color:#1f2937;'>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>1</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Date?.ToString("dd-MM-yyyy")}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.WeekCode}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Month}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.Project}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Section}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Line}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.StationName}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5;'>{savedRecord.ProblemStatement}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Frequency}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.IssueSeverity}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Category}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5; font-weight:600;'>{(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.CmDri}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.AppleDri}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.TypeOfAudit}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:700; color:#b45309;'>{savedRecord.Status}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>AFTER_IMAGE_PLACEHOLDER</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>

                </body>
                </html>";

            string? beforeImageFullPath = null;
            string? afterImageFullPath = null;

            if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
            {
                beforeImageFullPath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.BeforeImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
            {
                afterImageFullPath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.AfterImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
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
                    beforeImageFullPath,
                    afterImageFullPath
                );
            });

            TempData["SuccessMessage"] = "Checklist form data submitted and mail sent successfully";
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

            var savedRecord = await _service.GetByIdAsync(dto.Id);

            if (savedRecord == null)
            {
                TempData["ErrorMessage"] = "RCCA updated, but mail data not found.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            string subject = $"MIL Audit RCCA Updated {DateTime.Now:dd-MM-yyyy}";

            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                </head>
                <body style='margin:0; padding:0; background:#f4f6f8; font-family:Arial, sans-serif; color:#1f2937;'>

                    <div style='max-width:1200px; margin:0 auto; padding:24px;'>

                        <div style='background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; padding:22px;'>

                            <h2 style='margin:0 0 14px; color:#111827; font-size:20px;'>
                                MIL Audit RCCA Updated
                            </h2>

                            <p style='margin:0 0 10px; font-size:14px;'>Hi Team,</p>

                            <p style='margin:0 0 10px; font-size:14px;'>Good day.</p>

                            <p style='margin:0 0 20px; font-size:14px; line-height:1.6;'>
                                RCCA has been updated for the below MIL audit point. Please review the corrective action details.
                            </p>

                            <div style='overflow-x:auto; width:100%;'>
                                <table cellpadding='0' cellspacing='0'
                                    style='border-collapse:collapse; width:100%; min-width:1500px; table-layout:fixed; font-family:Arial, sans-serif; font-size:12px; text-align:center; border:1px solid #d1d5db;'>

                                    <thead>
                                        <tr style='background:#d7e5e1; color:#111827;'>
                                            <th style='width:60px; border:1px solid #c4cfd4; padding:10px 8px;'>S.NO</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Date</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Week<br/>Code</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Month</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Project</th>
                                            <th style='width:90px; border:1px solid #c4cfd4; padding:10px 8px;'>Section</th>
                                            <th style='width:80px; border:1px solid #c4cfd4; padding:10px 8px;'>Line</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Station Name</th>
                                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Problem Statement</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Frequency</th>
                                            <th style='width:110px; border:1px solid #c4cfd4; padding:10px 8px;'>Issue<br/>Severity</th>
                                            <th style='width:150px; border:1px solid #c4cfd4; padding:10px 8px;'>Category</th>
                                            <th style='width:220px; border:1px solid #c4cfd4; padding:10px 8px;'>Corrective Action</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Due Date</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>CM DRI</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Apple DRI</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Type of Audit</th>
                                            <th style='width:100px; border:1px solid #c4cfd4; padding:10px 8px;'>Status</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>Before Image</th>
                                            <th style='width:120px; border:1px solid #c4cfd4; padding:10px 8px;'>After Image</th>
                                        </tr>
                                    </thead>

                                    <tbody>
                                        <tr style='background:#ffffff; color:#1f2937;'>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>1</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Date?.ToString("dd-MM-yyyy")}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.WeekCode}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Month}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.Project}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Section}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Line}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.StationName}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5;'>{savedRecord.ProblemStatement}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Frequency}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:600;'>{savedRecord.IssueSeverity}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.Category}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; text-align:left; line-height:1.5; font-weight:600;'>{(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.CmDri}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.AppleDri}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>{savedRecord.TypeOfAudit}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px; font-weight:700; color:#b45309;'>{savedRecord.Status}</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>BEFORE_IMAGE_PLACEHOLDER</td>
                                            <td style='border:1px solid #e5e7eb; padding:10px 8px;'>AFTER_IMAGE_PLACEHOLDER</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>

                        </div>
                    </div>

                </body>
                </html>";

            string? beforeImageFullPath = null;
            string? afterImageFullPath = null;

            if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
            {
                beforeImageFullPath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.BeforeImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
            {
                afterImageFullPath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.AfterImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
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
                    beforeImageFullPath,
                    afterImageFullPath
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

            var records = await _service.GetAllAsync();
            var record = records.FirstOrDefault(x => x.Id == id);

            if (record == null)
            {
                TempData["ErrorMessage"] = "Checklist record not found.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            if (string.IsNullOrWhiteSpace(record.Rcca))
            {
                TempData["ErrorMessage"] = "Please enter RCCA before uploading After Image.";
                return RedirectToAction(nameof(ChecklistRecords));
            }

            if (afterImage == null || afterImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select or capture an After Image.";
                return RedirectToAction(nameof(ChecklistRecords));
            }


            string baseUrl = "http://10.52.16.17:5222";

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

            string subject = $"MIL Audit Review Required - {DateTime.Now:dd-MM-yyyy}";

            string? beforeImagePath = null;
            string? afterImagePath = null;

            if (!string.IsNullOrWhiteSpace(savedRecord.BeforeImagePath))
            {
                beforeImagePath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.BeforeImagePath.TrimStart('/')
                        .Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            if (!string.IsNullOrWhiteSpace(savedRecord.AfterImagePath))
            {
                afterImagePath = Path.Combine(
                    _env.WebRootPath,
                    savedRecord.AfterImagePath.TrimStart('/')
                        .Replace("/", Path.DirectorySeparatorChar.ToString())
                );
            }

            foreach (var recipient in recipients)
            {
                string encodedEmail = Uri.EscapeDataString(recipient);

                string approveUrl = $"{baseUrl}/Checklist/ApproveFromEmail?id={id}&email={encodedEmail}";
                string rejectUrl = $"{baseUrl}/Checklist/RejectFromEmail?id={id}&email={encodedEmail}";

                string body = $@"
                <html>
                <body style='font-family:Arial; background:#f7f7f7; padding:20px;'>

                    <h2>MIL Audit Review</h2>

                    <p>Hi Team,</p>

                    <p>
                        After Image has been uploaded successfully for the below audit point.
                        Please review and take action.
                    </p>

                    <div style='overflow-x:auto; width:100%;'>

                        <table cellpadding='0' cellspacing='0'
                               style='border-collapse:collapse;
                                      width:100%;
                                      min-width:1500px;
                                      table-layout:fixed;
                                      font-size:12px;
                                      text-align:center;
                                      border:1px solid #d1d5db;'>

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
                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:600;'>{savedRecord.Project}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Section}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Line}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.StationName}</td>

                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left;'>
                                        {savedRecord.ProblemStatement}
                                    </td>

                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Frequency}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:600;'>{savedRecord.IssueSeverity}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.Category}</td>

                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:left;'>
                                        {(string.IsNullOrWhiteSpace(savedRecord.Rcca) ? "-" : savedRecord.Rcca)}
                                    </td>

                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.DueDate?.ToString("dd-MM-yyyy")}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.CmDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.AppleDri}</td>
                                    <td style='border:1px solid #e5e7eb; padding:10px;'>{savedRecord.TypeOfAudit}</td>

                                    <td style='border:1px solid #e5e7eb; padding:10px; font-weight:700; color:#b45309;'>
                                        Ongoing
                                    </td>

                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:center; vertical-align:middle;'>
                                        BEFORE_IMAGE_PLACEHOLDER
                                    </td>
                                    <td style='border:1px solid #e5e7eb; padding:10px; text-align:center; vertical-align:middle;'>
                                        AFTER_IMAGE_PLACEHOLDER
                                    </td>

                                </tr>
                            </tbody>

                        </table>

                    </div>

                    <br />

                    <table role='presentation' style='margin-top:20px;'>
                        <tr>
                            <td bgcolor='#22c55e' style='border-radius:6px;'>
                                <a href='{approveUrl}' style='color:#fff; padding:15px; text-decoration:none; font-weight:bold;'>ACCEPT</a>
                            </td>

                            <td style='width:15px;'></td>

                            <td bgcolor='#ef4444' style='border-radius:6px;'>
                                <a href='{rejectUrl}' style='color:#fff; padding:15px; text-decoration:none; font-weight:bold;'>REJECT</a>
                            </td>
                        </tr>
                    </table>

                </body>
                </html>";

                _emailQueue.QueueEmail(async cancellationToken =>
                {
                    await _emailService.SendEmailWithInlineImagesAsync(
                        recipient,
                        subject,
                        body,
                        beforeImagePath,
                        afterImagePath
                    );
                });
            }

            TempData["SuccessMessage"] =
                "After Image uploaded successfully and approval mail sent.";

            return RedirectToAction(nameof(ChecklistRecords));
        }

        [HttpGet]
        public async Task<IActionResult> ApproveFromEmail(int id, string email)
        {
            await _service.MarkAcceptedAsync(id, email);

            bool allAccepted = await _service.AreAllAcceptedAsync(id);

            if (allAccepted)
            {
                await _service.UpdateStatusAsync(id, "Closed");
                TempData["SuccessMessage"] = "All users accepted. Status updated to CLOSED.";
            }
            else
            {
                await _service.UpdateStatusAsync(id, "Ongoing");
                TempData["SuccessMessage"] = "Accepted successfully. Waiting for other users.";
            }

            return View("ApprovalResult");
        }

        [HttpGet]
        public async Task<IActionResult> RejectFromEmail(int id, string email)
        {
            await _service.MarkRejectedAsync(id, email);
            await _service.UpdateStatusAsync(id, "Ongoing");

            TempData["ErrorMessage"] = "Rejected successfully. Status remains ONGOING.";

            return View("ApprovalResult");
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
        public async Task<IActionResult> SaveAuditHour(string AuditDateText, decimal AuditHour)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
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
            return RedirectToAction(nameof(AuditHours));
        }

    }
}