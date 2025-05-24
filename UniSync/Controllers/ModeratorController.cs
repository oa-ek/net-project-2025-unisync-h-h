using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UniSync.Models.Entity;
using UniSync.Repositories;
using UniSync.ViewModels;

namespace UniSync.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class ModeratorController : Controller
    {
        private readonly IContentReportRepository _reportRepository;
        private readonly INewsRepository _newsRepository;
        // Додайте інші репозиторії за необхідності

        public ModeratorController(
            IContentReportRepository reportRepository,
            INewsRepository newsRepository)
        {
            _reportRepository = reportRepository;
            _newsRepository = newsRepository;
        }

        // GET: /Moderator
        public async Task<IActionResult> Index()
        {
            var pendingReports = await _reportRepository.GetPendingReportsAsync();
            return View(pendingReports);
        }

        // GET: /Moderator/AllReports
        public async Task<IActionResult> AllReports()
        {
            var allReports = await _reportRepository.GetAllReportsAsync();
            return View(allReports);
        }

        // GET: /Moderator/ReportDetails/5
        public async Task<IActionResult> ReportDetails(int id)
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }

        // GET: /Moderator/ProcessReport/5
        public async Task<IActionResult> ProcessReport(int id)
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            var viewModel = new ContentReportViewModel
            {
                Id = report.Id,
                ContentType = report.ContentType,
                ContentId = report.ContentId,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
                ModeratorComment = report.ModeratorComment
            };

            return View(viewModel);
        }

        // POST: /Moderator/ProcessReport/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReport(int id, ContentReportViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var report = await _reportRepository.GetReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound();
                }

                report.Status = model.Status;
                report.ModeratorComment = model.ModeratorComment;
                report.ModeratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                report.ResolvedAt = DateTime.Now;

                // Якщо порушення підтверджено і це новина, можемо видалити її
                if (model.Status == ReportStatus.Confirmed && report.ContentType == ContentType.News)
                {
                    if (model.ContentId > 0)
                    {
                        await _newsRepository.DeleteNewsAsync(model.ContentId);
                    }
                }
                // Додайте обробку інших типів контенту за необхідності

                await _reportRepository.UpdateReportAsync(report);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Moderator/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.PendingReportsCount = await _reportRepository.GetPendingReportsCountAsync();
            var recentReports = await _reportRepository.GetPendingReportsAsync();
            return View(recentReports);
        }
    }
}