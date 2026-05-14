using Microsoft.Data.SqlClient;
using System.Text;

namespace E_MIL_Tracking_system.Services
{
    public class A2WeeklyReportBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public A2WeeklyReportBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _env = env;
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        var now = DateTime.Now;

        //        // Every Monday exactly 6 PM
        //        if (now.DayOfWeek == DayOfWeek.Monday &&
        //            now.Hour == 18 &&
        //            now.Minute == 0)
        //        {
        //            await SendPreviousWeekA2ReportAsync();

        //            // avoid sending multiple times within same minute/hour
        //            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        //        }
        //    }
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime? lastSentDate = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    if (now.Hour == 17 &&
                        now.Minute == 47 &&
                        lastSentDate != now.Date)
                    {
                        Console.WriteLine("A2 Weekly Report Mail Triggered");

                        await SendPreviousWeekA2ReportAsync();

                        lastSentDate = now.Date;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("A2 Weekly Report Error: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task SendPreviousWeekA2ReportAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            var today = DateTime.Today;

            var currentWeekMonday = today.AddDays(-(int)(today.DayOfWeek - DayOfWeek.Monday));
            var previousMonday = currentWeekMonday.AddDays(-7);
            var previousSaturday = previousMonday.AddDays(5);

            var records = new List<A2WeeklyReportDto>();

            string query = @"
                SELECT 
                    Id,
                    WeekCode,
                    Project,
                    Date,
                    Line,
                    StationName,
                    ProblemStatement,
                    BeforeImagePath,
                    AfterImagePath,
                    Frequency,
                    CmDri,
                    DueDate,
                    Rcca,
                    Status
                FROM ChecklistRecords
                WHERE CAST(Date AS DATE) >= @FromDate
                  AND CAST(Date AS DATE) <= @ToDate
                  AND ISNULL(LTRIM(RTRIM(Rcca)), '') <> ''
                  AND ISNULL(LTRIM(RTRIM(AfterImagePath)), '') <> ''
                ORDER BY Date ASC, Id ASC;
            ";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@FromDate", previousMonday);
                cmd.Parameters.AddWithValue("@ToDate", previousSaturday);

                await con.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    records.Add(new A2WeeklyReportDto
                    {
                        WeekCode = reader["WeekCode"]?.ToString(),
                        Project = reader["Project"]?.ToString(),
                        Date = reader["Date"] as DateTime?,
                        Line = reader["Line"]?.ToString(),
                        StationName = reader["StationName"]?.ToString(),
                        ProblemStatement = reader["ProblemStatement"]?.ToString(),
                        BeforeImagePath = reader["BeforeImagePath"]?.ToString(),
                        AfterImagePath = reader["AfterImagePath"]?.ToString(),
                        Frequency = reader["Frequency"]?.ToString(),
                        CmDri = reader["CmDri"]?.ToString(),
                        DueDate = reader["DueDate"] as DateTime?,
                        Rcca = reader["Rcca"]?.ToString(),
                        Status = reader["Status"]?.ToString()
                    });
                }
            }

            if (!records.Any())
                return;

            string subject = $"A2 Weekly Report - {previousMonday:dd-MMM-yyyy} to {previousSaturday:dd-MMM-yyyy}";

            string body = BuildA2WeeklyReportHtml(records, previousMonday, previousSaturday);

            await emailService.SendEmailAsync(
                "Sharath_G@foxlink.com",
                subject,
                body
            );
        }

        private string BuildA2WeeklyReportHtml(
            List<A2WeeklyReportDto> records,
            DateTime fromDate,
            DateTime toDate)
        {
            var sb = new StringBuilder();

            sb.Append($@"
                <html>
                <body style='font-family:Arial, sans-serif; background:#f4f6fb; padding:20px;'>
                    <h2 style='color:#1D2545;'>A2 Weekly Report</h2>

                    <p style='font-size:14px; line-height:1.8; color:#111827;'>

                        Hi sir,
                        <br/><br/>

                        Good Day ..!
                        <br/><br/>

                        Please find the updated Weekly Line Audit Reports for WK'19/04 attached.
                        I have uploaded the documents to Radar for your convenience.
                        <br/><br/>

                        <b>For A246c Radar Link :</b> rdar://162670324
                        <br/><br/>

                        Weekly E-System Validation report completed and uploaded to radar.
                        <br/><br/>

                        <b>For E-System Radar link :</b> rdar://163476850
                        <br/><br/>

                        <b>1. A246c Line Audit Points:</b>

                    </p>

                    <table cellpadding='8' cellspacing='0' style='border-collapse:collapse; width:100%; font-size:13px;'>
                        <thead>
                            <tr style='background:#d9dee8; color:#111827;'>
                                <th style='border:1px solid #7f8aa3;'>No.</th>
                                <th style='border:1px solid #7f8aa3;'>Week</th>
                                <th style='border:1px solid #7f8aa3;'>Project</th>
                                <th style='border:1px solid #7f8aa3;'>Date</th>
                                <th style='border:1px solid #7f8aa3;'>Problem Statement</th>
                                <th style='border:1px solid #7f8aa3;'>Picture Before</th>
                                <th style='border:1px solid #7f8aa3;'>Picture After</th>
                                <th style='border:1px solid #7f8aa3;'>Frequency</th>
                                <th style='border:1px solid #7f8aa3;'>DRI</th>
                                <th style='border:1px solid #7f8aa3;'>C/P</th>
                                <th style='border:1px solid #7f8aa3;'>Status</th>
                            </tr>
                        </thead>
                        <tbody>
            ");

            int serialNo = 1;

            foreach (var item in records)
            {
                sb.Append($@"
                    <tr>
                        <td style='border:1px solid #4b5563; text-align:center;'>{serialNo}</td>
                        <td style='border:1px solid #4b5563; text-align:center;'>{item.WeekCode}</td>
                        <td style='border:1px solid #4b5563; text-align:center;'>{item.Project}</td>
                        <td style='border:1px solid #4b5563; text-align:center;'>{item.Date?.ToString("d-MMM")}</td>

                        <td style='border:1px solid #4b5563; min-width:350px;'>
                            <b>Issue : {item.Line} : {item.StationName}</b><br/>
                            <b>Problem Statement :</b><br/>
                            {item.ProblemStatement}<br/><br/>
                            <b>Root Cause Corrective Action :</b><br/>
                            {item.Rcca}
                        </td>

                        <td style='border:1px solid #4b5563; text-align:center;'>
                            {BuildImageHtml(item.BeforeImagePath)}
                        </td>

                        <td style='border:1px solid #4b5563; text-align:center;'>
                            {BuildImageHtml(item.AfterImagePath)}
                        </td>

                        <td style='border:1px solid #4b5563; text-align:center;'>{item.Frequency}</td>
                        <td style='border:1px solid #4b5563; text-align:center;'>{item.CmDri}</td>
                        <td style='border:1px solid #4b5563; text-align:center;'>{item.DueDate?.ToString("d-MMM")}</td>
                        <td style='border:1px solid #4b5563; text-align:center; font-weight:700;'>
                            {item.Status}
                        </td>
                    </tr>
                ");

                serialNo++;
            }

            sb.Append(@"
                        </tbody>
                    </table>
                </body>
                </html>
            ");

            return sb.ToString();
        }

        private string BuildImageHtml(string? imagePaths)
        {
            if (string.IsNullOrWhiteSpace(imagePaths))
                return "-";

            string baseUrl = "http://10.52.1.126:8040/";

            var images = imagePaths.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            foreach (var img in images)
            {
                var path = img.Trim();

                string imageUrl = path.StartsWith("http")
                    ? path
                    : baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

                sb.Append($@"
                    <img src='{imageUrl}'
                         width='150'
                         height='100'
                         style='display:block;
                                object-fit:cover;
                                border-radius:6px;
                                border:1px solid #94a3b8;
                                margin:3px;
                                margin-left: 50px' />
                ");
            }

            return sb.Length == 0 ? "-" : sb.ToString();
        }

        private class A2WeeklyReportDto
        {
            public string? WeekCode { get; set; }
            public string? Project { get; set; }
            public DateTime? Date { get; set; }
            public string? Line { get; set; }
            public string? StationName { get; set; }
            public string? ProblemStatement { get; set; }
            public string? BeforeImagePath { get; set; }
            public string? AfterImagePath { get; set; }
            public string? Frequency { get; set; }
            public string? CmDri { get; set; }
            public DateTime? DueDate { get; set; }
            public string? Rcca { get; set; }
            public string? Status { get; set; }
        }
    }
}