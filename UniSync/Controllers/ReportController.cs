using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Ймовірно, не потрібен тут, якщо репозиторії абстрагують EF
using System;
using System.Linq; // Додайте для .Any()
using System.Security.Claims;
using System.Threading.Tasks;
using UniSync.Models.Entity;
using UniSync.Repositories;
using UniSync.ViewModels;

namespace UniSync.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IContentReportRepository _reportRepository;
        private readonly INewsRepository _newsRepository;
       

        public ReportController(IContentReportRepository reportRepository, INewsRepository newsRepository /*, ICommentRepository commentRepository */)
        {
            _reportRepository = reportRepository;
            _newsRepository = newsRepository;
            // _commentRepository = commentRepository; // Приклад
        }

        [HttpGet]
        public async Task<IActionResult> Create(ContentType? contentType, int? contentId, string contentTitle = null)
        {
            var viewModel = new ContentReportViewModel();
            string actualContentTitle = contentTitle;

            if (contentType.HasValue && contentId.HasValue)
            {
                // Випадок, коли репорт створюється для конкретного контенту
                if (!await CheckContentExists(contentType.Value, contentId.Value))
                {
                    TempData["ErrorMessage"] = "Зазначений контент для репорту не знайдено.";
                    return RedirectToAction("Index", "Home"); // Або інша сторінка помилки/повідомлення
                }
                viewModel.ContentType = contentType.Value;
                viewModel.ContentId = contentId.Value;

                // Спробуйте отримати більш точний заголовок, якщо він не був переданий
                if (string.IsNullOrEmpty(actualContentTitle))
                {
                    actualContentTitle = await GetContentTitleAsync(contentType.Value, contentId.Value);
                }
            }

            ViewBag.ContentTitle = actualContentTitle; // Може бути null, якщо це загальний репорт
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentReportViewModel model)
        {
            if (!await CheckContentExists(model.ContentType, model.ContentId))
            {
                ViewBag.ContentTitle = await GetContentTitleAsync(model.ContentType, model.ContentId); // Повторно для випадку помилки ModelState
                ModelState.AddModelError(string.Empty, "Обраний контент не знайдено. Можливо, він був видалений.");
                return View(model); // Повертаємо View з помилкою
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ContentTitle = await GetContentTitleAsync(model.ContentType, model.ContentId);
                return View(model);
            }

            var reporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(reporterId)) return Unauthorized();

            var existingReports = await _reportRepository.GetReportsByContentAsync(model.ContentType, model.ContentId);
            if (existingReports.Any(r => r.ReporterId == reporterId))
            {
                TempData["ErrorMessage"] = "Ви вже подавали звіт щодо цього контенту.";
                ViewBag.ContentTitle = await GetContentTitleAsync(model.ContentType, model.ContentId); // Потрібно для View
                return View(model);
            }

            var report = new ContentReport
            {
                ContentType = model.ContentType,
                ContentId = model.ContentId,
                Reason = model.Reason,
                Description = model.Description,
                CreatedAt = DateTime.Now, // Краще використовувати DateTime.UtcNow для серверного часу
                Status = ReportStatus.Pending,
                ReporterId = reporterId
            };

            await _reportRepository.AddReportAsync(report);
            TempData["SuccessMessage"] = "Дякуємо за повідомлення. Модератори розглянуть його найближчим часом.";
            return RedirectToContentOrigin(model.ContentType, model.ContentId);
        }

        private async Task<bool> CheckContentExists(ContentType contentType, int contentId)
        {
            if (contentId <= 0) return false; // ID не може бути 0 або негативним

            return contentType switch
            {
                ContentType.News => await _newsRepository.GetNewsByIdAsync(contentId) != null,

                _ => false
            };
        }

        private async Task<string> GetContentTitleAsync(ContentType contentType, int contentId)
        {
            if (contentId <= 0) return null;

            switch (contentType)
            {
                case ContentType.News:
                    var newsItem = await _newsRepository.GetNewsByIdAsync(contentId);
                    return newsItem?.Title;
                default:
                    return $"Контент типу '{contentType}' з ID {contentId}"; // Загальний заголовок
            }
        }

        private IActionResult RedirectToContentOrigin(ContentType contentType, int contentId)
        {
            return contentType switch
            {
                ContentType.News => RedirectToAction("Details", "News", new { id = contentId }),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public async Task<IActionResult> MyReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(); // Додаткова перевірка
            var reports = await _reportRepository.GetReportsByUserAsync(userId);
            return View(reports);
        }
    }
}