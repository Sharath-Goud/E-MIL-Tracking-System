using E_MIL_Tracking_system.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_MIL_Tracking_system.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ChecklistService _checklistService;

        public ReportsController(ChecklistService checklistService)
        {
            _checklistService = checklistService;
        }

        public async Task<IActionResult> Reports()
        {
            var records = await _checklistService.GetAllAsync();
            return View(records);
        }
    }
}