using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UniSync.Models.Entity;
using UniSync.Repositories;
using UniSync.ViewModels;

namespace UniSync.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsRepository _newsRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(INewsRepository newsRepository, IWebHostEnvironment webHostEnvironment)
        {
            _newsRepository = newsRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /News
        public async Task<IActionResult> Index()
        {
            var news = await _newsRepository.GetAllNewsAsync();
            return View(news);
        }

        // GET: /News/Updates
        public async Task<IActionResult> Updates()
        {
            var news = await _newsRepository.GetLatestNewsAsync(10);
            return View(news);
        }

        // GET: /News/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var news = await _newsRepository.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // GET: /News/Category/1
        public async Task<IActionResult> Category(int category)
        {
            var newsCategory = (NewsCategory)category;
            ViewBag.Category = newsCategory;
            var news = await _newsRepository.GetNewsByCategoryAsync(newsCategory);
            return View(news);
        }

        // GET: /News/Create
        [Authorize(Roles = "Admin,NewsEditor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> Create(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var news = new News
                {
                    Title = model.Title,
                    Content = model.Content,
                    Summary = model.Summary,
                    IsImportant = model.IsImportant,
                    Category = model.Category,
                    PublishedDate = DateTime.Now,
                    AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    AuthorName = User.Identity.Name
                };

                // Обробка зображення
                if (model.Image != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    news.ImageUrl = "/images/news/" + uniqueFileName;
                }

                await _newsRepository.AddNewsAsync(news);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /News/Edit/5
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsRepository.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            var viewModel = new NewsViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                Summary = news.Summary,
                IsImportant = news.IsImportant,
                Category = news.Category
            };

            ViewBag.CurrentImage = news.ImageUrl;

            return View(viewModel);
        }

        // POST: /News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> Edit(int id, NewsViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var news = await _newsRepository.GetNewsByIdAsync(id);
                if (news == null)
                {
                    return NotFound();
                }

                news.Title = model.Title;
                news.Content = model.Content;
                news.Summary = model.Summary;
                news.IsImportant = model.IsImportant;
                news.Category = model.Category;

                // Обробка зображення
                if (model.Image != null)
                {
                    // Видалення старого зображення, якщо воно існує
                    if (!string.IsNullOrEmpty(news.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "news");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    news.ImageUrl = "/images/news/" + uniqueFileName;
                }

                await _newsRepository.UpdateNewsAsync(news);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /News/Delete/5
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsRepository.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: /News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _newsRepository.GetNewsByIdAsync(id);
            if (news != null)
            {
                // Видалення зображення, якщо воно існує
                if (!string.IsNullOrEmpty(news.ImageUrl))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                await _newsRepository.DeleteNewsAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /News/Manage
        [Authorize(Roles = "Admin,NewsEditor")]
        public async Task<IActionResult> Manage()
        {
            var news = await _newsRepository.GetAllNewsAsync();
            return View(news);
        }

        // GET: /News/GetLatestNews
        [HttpGet]
        public async Task<IActionResult> GetLatestNews()
        {
            var latestNews = await _newsRepository.GetLatestNewsAsync(5);
            return Json(latestNews);
        }
    }
}