using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniSync.Data;
using UniSync.Models.Entity;
using UniSync.Areas.Identity.Data;

namespace UniSync.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UniSyncContext _context;
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            UniSyncContext context,
            UserManager<UniSyncUser> userManager,
            ILogger<ProjectsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            try
            {
                // Отримуємо ID поточного користувача
                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"Отримання проектів для користувача: {userId}");

                // Отримуємо тільки проекти, які належать поточному користувачу
                var projects = await _context.Projects
                    .Include(p => p.Subject)
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation($"Знайдено {projects.Count} проектів");
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку проектів");
                TempData["ErrorMessage"] = "Виникла помилка при завантаженні проектів. Спробуйте пізніше.";
                return View(new List<Project>());
            }
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"Підготовка до створення проекту для користувача: {userId}");

                // Перевіряємо, чи є у користувача предмети
                var subjects = await _context.Subjects
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation($"Знайдено {subjects.Count} предметів для користувача");

                if (subjects.Count == 0)
                {
                    // Якщо предметів немає, створюємо стандартний предмет для користувача
                    _logger.LogInformation("Створення стандартного предмету для користувача");
                    var defaultSubject = new Subject
                    {
                        Title = "Загальний предмет",
                        UserId = userId
                    };

                    _context.Subjects.Add(defaultSubject);
                    await _context.SaveChangesAsync();

                    subjects.Add(defaultSubject);
                    _logger.LogInformation($"Створено стандартний предмет з ID: {defaultSubject.Id}");
                }

                ViewBag.SubjectId = new SelectList(subjects, "Id", "Title");
                return View(new Project { Progress = 0, Status = "Not Started", Priority = "Medium" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при підготовці до створення проекту");
                TempData["ErrorMessage"] = "Виникла помилка при підготовці форми створення проекту. Спробуйте пізніше.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectId,Title,Priority,Deadline,Status,Description,Progress")] Project project)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"Спроба створення проекту для користувача: {currentUser.Id}");
            _logger.LogInformation($"Дані проекту: SubjectId={project.SubjectId}, Title={project.Title}, Priority={project.Priority}, Status={project.Status}, Progress={project.Progress}");

            var subject = await _context.Subjects.FindAsync(project.SubjectId);
            if (subject == null)
            {
                _logger.LogWarning($"Предмет з ID {project.SubjectId} не знайдено");
                ModelState.AddModelError("SubjectId", "Вибраний предмет не існує");
            }
            else if (subject.UserId != currentUser.Id)
            {
                _logger.LogWarning($"Предмет з ID {project.SubjectId} не належить користувачу {currentUser.Id}");
                ModelState.AddModelError("SubjectId", "Вибраний предмет не належить вам");
            }

            // Видаляємо помилки валідації для полів, які ми заповнюємо вручну
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Subject");
            ModelState.Remove("CreatedAt"); // Обов’язкове поле, але ми його встановлюємо

            if (ModelState.IsValid)
            {
                try
                {
                    project.UserId = currentUser.Id;
                    project.CreatedAt = DateTime.Now;

                    _logger.LogInformation("Модель валідна, додавання проекту до бази даних");
                    _context.Add(project);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Проект успішно створено з ID: {project.Id}");
                    TempData["SuccessMessage"] = "Проект успішно створено!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка при збереженні проекту");
                    ModelState.AddModelError("", $"Помилка при створенні проекту: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Модель недійсна. Помилки валідації:");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogWarning($"Поле: {modelState.Key}, Помилка: {error.ErrorMessage}");
                    }
                }
            }

            ViewBag.SubjectId = new SelectList(await _context.Subjects.Where(s => s.UserId == currentUser.Id).ToListAsync(), "Id", "Title", project.SubjectId);
            return View(project);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                var project = await _context.Projects
                    .Include(p => p.Subject)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                // Перевірка, чи проект належить поточному користувачу
                if (project.UserId != userId)
                {
                    return Forbid(); // Заборона доступу, якщо проект не належить користувачу
                }

                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні деталей проекту з ID: {id}");
                TempData["ErrorMessage"] = "Виникла помилка при завантаженні деталей проекту. Спробуйте пізніше.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                var project = await _context.Projects
                    .Include(p => p.Subject)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                // Перевірка, чи проект належить поточному користувачу
                if (project.UserId != userId)
                {
                    return Forbid(); // Заборона доступу, якщо проект не належить користувачу
                }

                ViewBag.SubjectId = new SelectList(await _context.Subjects.Where(s => s.UserId == userId).ToListAsync(), "Id", "Title", project.SubjectId);
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні проекту для редагування з ID: {id}");
                TempData["ErrorMessage"] = "Виникла помилка при завантаженні проекту для редагування. Спробуйте пізніше.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectId,CreatedAt,Title,Priority,Deadline,Status,Description,Progress,UserId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            try
            {
                // Перевірка, чи існуючий проект належить поточному користувачу
                var existingProject = await _context.Projects.FindAsync(id);
                if (existingProject == null)
                {
                    return NotFound();
                }

                if (existingProject.UserId != userId)
                {
                    return Forbid(); // Заборона доступу, якщо проект не належить користувачу
                }

                // Перевірка, чи існує предмет і чи належить він поточному користувачу
                var subject = await _context.Subjects.FindAsync(project.SubjectId);
                if (subject == null)
                {
                    ModelState.AddModelError("SubjectId", "Вибраний предмет не існує");
                    ViewBag.SubjectId = new SelectList(await _context.Subjects.Where(s => s.UserId == userId).ToListAsync(), "Id", "Title", project.SubjectId);
                    return View(project);
                }

                if (subject.UserId != userId)
                {
                    ModelState.AddModelError("SubjectId", "Вибраний предмет не належить вам");
                    ViewBag.SubjectId = new SelectList(await _context.Subjects.Where(s => s.UserId == userId).ToListAsync(), "Id", "Title", project.SubjectId);
                    return View(project);
                }

                // Зберігаємо оригінальний UserId
                project.UserId = userId;

                if (ModelState.IsValid)
                {
                    try
                    {
                        project.UpdatedAt = DateTime.Now;

                        _context.Update(project);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Проект з ID: {project.Id} успішно оновлено");
                        TempData["SuccessMessage"] = "Проект успішно оновлено!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!ProjectExists(project.Id))
                        {
                            _logger.LogWarning($"Проект з ID: {project.Id} не знайдено при спробі оновлення");
                            return NotFound();
                        }
                        else
                        {
                            _logger.LogError(ex, $"Помилка конкурентності при оновленні проекту з ID: {project.Id}");
                            throw;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Модель недійсна при редагуванні проекту. Помилки валідації:");
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            _logger.LogWarning($"Поле: {modelState.Key}, Помилка: {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при оновленні проекту з ID: {project.Id}");
                ModelState.AddModelError("", $"Помилка при оновленні проекту: {ex.Message}");
            }

            ViewBag.SubjectId = new SelectList(await _context.Subjects.Where(s => s.UserId == userId).ToListAsync(), "Id", "Title", project.SubjectId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var userId = _userManager.GetUserId(User);

                var project = await _context.Projects
                    .Include(p => p.Subject)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                // Перевірка, чи проект належить поточному користувачу
                if (project.UserId != userId)
                {
                    return Forbid(); // Заборона доступу, якщо проект не належить користувачу
                }

                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при отриманні проекту для видалення з ID: {id}");
                TempData["ErrorMessage"] = "Виникла помилка при завантаженні проекту для видалення. Спробуйте пізніше.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var project = await _context.Projects
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (project == null)
                {
                    return NotFound();
                }

                if (project.UserId != userId)
                {
                    return Forbid();
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Проект з ID: {id} успішно видалено");
                TempData["SuccessMessage"] = "Проект успішно видалено!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при видаленні проекту з ID: {id}");
                TempData["ErrorMessage"] = "Виникла помилка при видаленні проекту. Спробуйте пізніше.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}