using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniSync.Areas.Identity.Data;
using UniSync.Constants;
using UniSync.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace UniSync.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<UniSyncUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> EditRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                Roles = Roles.AllRoles.Select(r => new RoleViewModel
                {
                    Name = r,
                    IsSelected = userRoles.Contains(r)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            bool isSuperAdmin = await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);
            var superAdminRole = model.Roles.FirstOrDefault(r => r.Name == Roles.SuperAdmin);

            if (isSuperAdmin && (superAdminRole == null || !superAdminRole.IsSelected))
            {
                TempData["ErrorMessage"] = "Неможливо видалити роль SuperAdmin у суперадміністратора";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            foreach (var role in model.Roles.Where(r => r.IsSelected))
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }

            TempData["SuccessMessage"] = "Ролі користувача успішно оновлено";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string id, string reason, string duration, string comment, bool notifyUser = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Ідентифікатор користувача не вказано";
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Користувача не знайдено";
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
            {
                TempData["ErrorMessage"] = "Неможливо заблокувати суперадміністратора";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Поточний користувач не знайдений";
                return RedirectToAction(nameof(Index));
            }

            if (currentUser.Id == id)
            {
                TempData["ErrorMessage"] = "Ви не можете заблокувати самого себе";
                return RedirectToAction(nameof(Index));
            }

            DateTimeOffset? lockoutEnd;
            if (duration == "permanent")
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            else if (int.TryParse(duration, out int days) && days > 0)
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
            }
            else
            {
                _logger.LogWarning("Невірна тривалість блокування '{Duration}' для користувача {UserId}", duration, id);
                TempData["ErrorMessage"] = "Невірно вказана тривалість блокування. Вкажіть 'permanent' або кількість днів.";
                return RedirectToAction(nameof(Index));
            }

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!lockoutResult.Succeeded)
            {
                _logger.LogError("Не вдалося встановити блокування для користувача {UserId}: {Errors}",
                    id, string.Join(", ", lockoutResult.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Помилка при блокуванні користувача";
                return RedirectToAction(nameof(Index));
            }

            if (user is UniSyncUser appUser)
            {
                appUser.LockoutReason = reason;
                appUser.LockoutComment = comment;
                var updateResult = await _userManager.UpdateAsync(appUser);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Не вдалося оновити причину та коментар блокування для користувача {UserId}: {Errors}",
                        id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Блокування виконано, але не вдалося зберегти причину або коментар";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _logger.LogWarning("Користувач {UserId} не є UniSyncUser, пропущено збереження причини та коментаря", id);
            }

            _logger.LogInformation("Користувач {UserId} заблокований адміністратором {AdminId}. Причина: {Reason}, Тривалість: {Duration}, Коментар: {Comment}",
                id, currentUser.Id, reason, duration, comment);

            if (notifyUser)
            {
                _logger.LogInformation("Сповіщення для користувача {UserId} не реалізовано", id);
            }

            TempData["SuccessMessage"] = "Користувача успішно заблоковано";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Ідентифікатор користувача не вказано";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
            {
                TempData["ErrorMessage"] = "Користувач не заблокований";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                if (user is UniSyncUser appUser)
                {
                    appUser.LockoutReason = null;
                    appUser.LockoutComment = null;
                    await _userManager.UpdateAsync(appUser);
                }

                _logger.LogInformation("Користувач {UserId} розблокований адміністратором {AdminId}",
                 id, (await _userManager.GetUserAsync(User))?.Id ?? "Unknown");

                TempData["SuccessMessage"] = "Користувача успішно розблоковано";
            }
            else
            {
                _logger.LogError("Не вдалося розблокувати користувача {UserId}: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Помилка при розблокуванні користувача";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifySuperAdminPassword([FromBody] SuperAdminPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false });
            }

            // Отримуємо поточного користувача
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false });
            }

            // Перевіряємо, чи є поточний користувач адміністратором
            if (!await _userManager.IsInRoleAsync(currentUser, Roles.Admin) &&
                !await _userManager.IsInRoleAsync(currentUser, Roles.SuperAdmin))
            {
                _logger.LogWarning("Спроба перевірки пароля для SuperAdmin від користувача без прав адміністратора: {UserId}", currentUser.Id);
                return Json(new { success = false });
            }

            // Перевіряємо пароль
            var isPasswordValid = await _userManager.CheckPasswordAsync(currentUser, model.Password);

            if (isPasswordValid)
            {
                _logger.LogInformation("Успішна перевірка пароля для надання ролі SuperAdmin користувачем {UserId}", currentUser.Id);
                return Json(new { success = true });
            }
            else
            {
                _logger.LogWarning("Невдала спроба перевірки пароля для надання ролі SuperAdmin користувачем {UserId}", currentUser.Id);
                return Json(new { success = false });
            }
        }
    }
}