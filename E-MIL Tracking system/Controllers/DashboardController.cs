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
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var records = await _service.GetAllAsync();

            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            var validRecords = records
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Month == selectedMonth &&
                            x.Date.Value.Year == selectedYear)
                .ToList();

            var dateLabels = validRecords
                .OrderBy(x => x.Date)
                .Select(x => x.Date!.Value.ToString("M/dd"))
                .Distinct()
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
                    data = dateLabels.Select(date =>
                        validRecords.Count(x =>
                            x.Date!.Value.ToString("M/dd") == date &&
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
                    data = dateLabels.Select(date =>
                        validRecords.Count(x =>
                            x.Date!.Value.ToString("M/dd") == date &&
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
                    data = dateLabels.Select(date =>
                        validRecords.Count(x =>
                            x.Date!.Value.ToString("M/dd") == date &&
                            NormalizeCategory(x.Category) == category &&
                            validRecords.Count(y =>
                                y.Date!.Value.ToString("M/dd") == date &&
                                NormalizeCategory(y.Category) == category &&
                                string.Equals(y.StationName?.Trim(), x.StationName?.Trim(), StringComparison.OrdinalIgnoreCase)
                            ) > 1
                        )
                    ).ToList()
                })
                .ToList();

            var selectedDate = new DateTime(selectedYear, selectedMonth, 1);
            var selectedWeek = System.Globalization.ISOWeek.GetWeekOfYear(selectedDate);

            var weekNumbers = Enumerable.Range(selectedWeek, 26)
                .Where(w => w <= 52)
                .ToList();

            var weeklyData = weekNumbers.Select(week => new
            {
                Week = week,
                Count = records.Count(x =>
                    x.Date.HasValue &&
                    x.Date.Value.Year == selectedYear &&
                    System.Globalization.ISOWeek.GetWeekOfYear(x.Date.Value) == week)
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

            return View();
        }
    }
}