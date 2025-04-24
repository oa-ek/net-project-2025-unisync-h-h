using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UniSync.Areas.Identity.Data;
using UniSync.Models.ViewModels;

namespace UniSync.Controllers
{
    public class AccountStatusController : Controller
    {
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly ILogger<AccountStatusController> _logger;

        public AccountStatusController(UserManager<UniSyncUser> userManager, ILogger<AccountStatusController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Locked(string email = null)
        {
            _logger.LogInformation("Дія Locked викликана");

            UniSyncUser user = null;
            if (!string.IsNullOrEmpty(email))
            {
                user = await _userManager.FindByEmailAsync(email);
                _logger.LogInformation("Отримано email: {Email}, користувач знайдений: {UserFound}", email, user != null);
            }

            if (user == null)
            {
                user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Спроба доступу до сторінки блокування без автентифікації та без email");
                    return RedirectToPage("/Account/Login", new { area = "Identity", error = "AccountLocked" });
                }
            }

            _logger.LogInformation($"Користувач: {user.Id}, LockoutEnd: {user.LockoutEnd}, Поточна дата: {DateTimeOffset.UtcNow}");
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var model = new LockedAccountViewModel
                {
                    UserName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email ?? string.Empty,
                    LockoutEnd = user.LockoutEnd,
                    LockoutReason = user.LockoutReason ?? "Порушення правил користування сайтом"
                };

                _logger.LogInformation("Заблокований користувач {UserId} переглядає сторінку блокування", user.Id);
                return View("~/Views/UserManagement/Locked.cshtml", model);
            }

            _logger.LogInformation("Користувач не заблокований, перенаправлення на головну");
            return RedirectToAction("Index", "Home");
        }
    }
}