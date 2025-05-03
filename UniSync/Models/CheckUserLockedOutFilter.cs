using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UniSync.Areas.Identity.Data;
using UniSync.Controllers;

namespace UniSync.Models
{
    public class CheckUserLockedOutFilter : IAsyncActionFilter
    {
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly ILogger<CheckUserLockedOutFilter> _logger;

        public CheckUserLockedOutFilter(
            UserManager<UniSyncUser> userManager,
            ILogger<CheckUserLockedOutFilter> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("CheckUserLockedOutFilter викликаний для дії: {Action}", context.ActionDescriptor.DisplayName);

            // Пропускаємо перевірку для AccountStatus контролера
            if (context.Controller is AccountStatusController)
            {
                _logger.LogInformation("Пропускаємо перевірку для AccountStatusController");
                await next();
                return;
            }

            // Пропускаємо перевірку для неавторизованих користувачів
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogInformation("Користувач не автентифікований, пропускаємо перевірку");
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            if (user == null)
            {
                _logger.LogWarning("Користувач не знайдений");
                await next();
                return;
            }

            _logger.LogInformation($"Перевірка блокування для користувача {user.Id}. LockoutEnd: {user.LockoutEnd}, Поточна дата: {DateTimeOffset.UtcNow}");
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("Заблокований користувач {UserId} перенаправлений на сторінку блокування", user.Id);
                context.Result = new RedirectToActionResult("Locked", "AccountStatus", null);
                return;
            }

            _logger.LogInformation("Користувач не заблокований, продовжуємо");
            await next();
        }
    }
}