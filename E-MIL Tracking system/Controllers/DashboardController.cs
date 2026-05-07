using E_MIL_Tracking_system.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_MIL_Tracking_system.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ChecklistService _service;

        public DashboardController(ChecklistService service)
        {
            _service = service;
        }

        private static string NormalizeCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "Others";

            var value = category.Trim().ToLower();

            if (value == "5s/esd")
                return "5S/ESD";

            if (value.Contains("maintenance") || value.Contains("calibrition") || value.Contains("calibration"))
                return "Maintenance/calibration/settings";

            if (value.Contains("sip") || value.Contains("sop"))
                return "SOP/SIP Compliance";

            return "Others";
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? month, int? year, int? week)
        {
            var records = await _service.GetAllAsync();

            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedWeekFilter = week ?? 1;

            if (selectedWeekFilter < 1 || selectedWeekFilter > 4)
            {
                selectedWeekFilter = 1;
            }

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedWeek = selectedWeekFilter;

            var monthlyRecords = records
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Month == selectedMonth &&
                            x.Date.Value.Year == selectedYear)
                .ToList();

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var selectedWeekStart = firstDayOfMonth.AddDays((selectedWeekFilter - 1) * 7);

            var chartDates = Enumerable.Range(0, 7)
                .Select(i => selectedWeekStart.AddDays(i).Date)
                .Where(d =>
                    d.Month == selectedMonth &&
                    d.Year == selectedYear &&
                    d.DayOfWeek != DayOfWeek.Sunday)
                .Take(6)
                .ToList();

            var validRecords = monthlyRecords
                .Where(x => x.Date.HasValue &&
                            chartDates.Contains(x.Date.Value.Date))
                .ToList();

            var dateLabels = chartDates
                .Select(d => d.ToString("M-dd"))
                .ToList();

            ViewBag.StatusSummary = validRecords
                .Where(x => !string.IsNullOrWhiteSpace(x.Status))
                .GroupBy(x => x.Status!.Trim())
                .Select(g => new
                {
                    name = g.Key,
                    y = g.Count()
                })
                .ToList();

            ViewBag.SeverityCategories = dateLabels;

            ViewBag.SeveritySeries = new[] { "Minor", "Major", "Critical" }
                .Select(severity => new
                {
                    name = severity,
                    data = chartDates.Select(chartDate =>
                        validRecords.Count(x =>
                            x.Date.HasValue &&
                            x.Date.Value.Date == chartDate &&
                            string.Equals(x.IssueSeverity?.Trim(), severity, StringComparison.OrdinalIgnoreCase)
                        )
                    ).ToList()
                })
                .ToList();

            var categoryNames = new[]
            {
        "5S/ESD",
        "Maintenance/calibration/settings",
        "SOP/SIP Compliance",
        "Others"
    };

            ViewBag.CategoryCategories = dateLabels;

            ViewBag.CategorySeries = categoryNames
                .Select(category => new
                {
                    name = category,
                    data = chartDates.Select(chartDate =>
                        validRecords.Count(x =>
                            x.Date.HasValue &&
                            x.Date.Value.Date == chartDate &&
                            NormalizeCategory(x.Category) == category
                        )
                    ).ToList()
                })
                .ToList();

            ViewBag.RepeatedIssueDepartments = dateLabels;

            ViewBag.RepeatedIssueSeries = categoryNames
            .Select(category => new
            {
                name = category,
                data = chartDates.Select(chartDate =>
                    validRecords.Count(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date == chartDate &&
                        string.Equals(x.IssueType?.Trim(), "Repeated", StringComparison.OrdinalIgnoreCase) && 
                        NormalizeCategory(x.Category) == category
                    )
                ).ToList()
            })
            .ToList();

            int daysToMonday =
                ((int)DayOfWeek.Monday - (int)firstDayOfMonth.DayOfWeek + 7) % 7;

            var firstMonday = firstDayOfMonth.AddDays(daysToMonday);

            var weekNumbers = Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var weekStart = firstMonday.AddDays(i * 7);

                    if (weekStart > lastDayOfMonth)
                    {
                        return (int?)null;
                    }

                    return System.Globalization.ISOWeek.GetWeekOfYear(weekStart);
                })
                .Where(w => w.HasValue)
                .Select(w => w!.Value)
                .ToList();

            var weeklyData = weekNumbers.Select(weekNo => new
            {
                Week = weekNo,
                Count = monthlyRecords.Count(x =>
                    x.Date.HasValue &&
                    System.Globalization.ISOWeek.GetWeekOfYear(x.Date.Value) == weekNo)
            }).ToList();

            ViewBag.WeeklyReportCategories = weeklyData
                .Select(x => "WK" + x.Week)
                .ToList();

            ViewBag.WeeklyReportSeries = new[]
            {
        new
        {
            name = "Finding/Hour",
            data = weeklyData.Select(x => x.Count).ToList()
        }
    };

            var auditHours = await _service.GetAuditHoursAsync();

            var weeklyFindingPerHourData = weekNumbers.Select(weekNo =>
            {
                var weekStart = System.Globalization.ISOWeek.ToDateTime(
                    selectedYear,
                    weekNo,
                    DayOfWeek.Monday
                );

                decimal totalFindingSum = 0;
                decimal auditHourSum = 0;

                for (int i = 0; i <= 5; i++)
                {
                    var currentDate = weekStart.AddDays(i).Date;

                    int dayFinding = records.Count(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date == currentDate);

                    totalFindingSum += dayFinding;

                    var savedAudit = auditHours.FirstOrDefault(x =>
                        x.ParsedDate.HasValue &&
                        x.ParsedDate.Value.Date == currentDate);

                    if (savedAudit != null)
                    {
                        auditHourSum += savedAudit.AuditHour;
                    }
                }

                if (auditHourSum > 0)
                {
                    decimal result = totalFindingSum / auditHourSum;
                    return Math.Floor(result * 10) / 10;
                }

                return 0;
            }).ToList();

            ViewBag.WeeklyFindingPerHourSeries = new[]
            {
        new
        {
            name = "Finding/Hour",
            data = weeklyFindingPerHourData
        }
    };

            var weeklyRData = weekNumbers.Select(weekNo =>
            {
                var weekStart = System.Globalization.ISOWeek.ToDateTime(
                    selectedYear,
                    weekNo,
                    DayOfWeek.Monday
                );

                var findingPerHourValues = new List<decimal>();

                for (int i = 0; i <= 5; i++)
                {
                    var currentDate = weekStart.AddDays(i).Date;

                    int dayFinding = records.Count(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date == currentDate);

                    var savedAudit = auditHours.FirstOrDefault(x =>
                        x.ParsedDate.HasValue &&
                        x.ParsedDate.Value.Date == currentDate);

                    if (savedAudit != null && savedAudit.AuditHour > 0)
                    {
                        decimal findingPerHour = dayFinding / savedAudit.AuditHour;
                        decimal truncated = Math.Floor(findingPerHour * 10) / 10;

                        if (truncated > 0)
                        {
                            findingPerHourValues.Add(truncated);
                        }   
                    }
                }

                if (findingPerHourValues.Any())
                {
                    decimal maxValue = findingPerHourValues.Max();
                    decimal minValue = findingPerHourValues.Min();

                    decimal rValue = maxValue - minValue;
                    return Math.Floor(rValue * 10) / 10;
                }

                return 0;
            }).ToList();

            ViewBag.WeeklyRChartSeries = new[]
            {
        new
        {
            name = "R-Chart",
            data = weeklyRData
        }
    };

            var auditTableRows = weekNumbers.Select(weekNo =>
            {
                var weekStart = System.Globalization.ISOWeek.ToDateTime(
                    selectedYear,
                    weekNo,
                    DayOfWeek.Monday
                );

                var dates = new List<string> { $"WK-{weekNo:00}/{weekStart:dd-MMM}" };
                dates.AddRange(Enumerable.Range(1, 5).Select(i => weekStart.AddDays(i).ToString("dd-MMM")));

                var totalFindings = new List<string>();
                var auditHourValues = new List<string>();

                for (int i = 0; i <= 5; i++)
                {
                    var currentDate = weekStart.AddDays(i).Date;

                    totalFindings.Add(records.Count(x =>
                        x.Date.HasValue &&
                        x.Date.Value.Date == currentDate
                    ).ToString());

                    var savedAudit = auditHours.FirstOrDefault(x =>
                        x.ParsedDate.HasValue &&
                        x.ParsedDate.Value.Date == currentDate);

                    auditHourValues.Add(savedAudit?.AuditHour.ToString("0.##") ?? "");
                }

                decimal totalFindingSum = totalFindings.Sum(x =>
                {
                    decimal.TryParse(x, out var value);
                    return value;
                });

                decimal auditHourSum = auditHourValues.Sum(x =>
                {
                    decimal.TryParse(x, out var value);
                    return value;
                });

                string findingPerHour = "";

                if (auditHourSum > 0)
                {
                    decimal result = totalFindingSum / auditHourSum;
                    findingPerHour = (Math.Floor(result * 10) / 10).ToString("0.0");
                }

                var findingHr = new List<string> { "", "", "", "", "", findingPerHour };

                var validValues = findingHr
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x =>
                    {
                        decimal.TryParse(x, out var value);
                        return value;
                    })
                    .Where(x => x > 0)
                    .ToList();

                string rValue = "";

                if (validValues.Any())
                {
                    decimal r = validValues.Max() - validValues.Min();
                    rValue = (Math.Floor(r * 10) / 10).ToString("0.0");
                }

                return new
                {
                    Week = $"WK-{weekNo:00}",
                    Dates = dates,
                    TotalFindings = totalFindings,
                    AuditHours = auditHourValues,
                    FindingHr = findingHr,
                    RValues = new List<string> { "", "", "", "", "", rValue }
                };
            }).ToList();

            ViewBag.AuditTableRows = auditTableRows;

            return View();
        }
    }
}