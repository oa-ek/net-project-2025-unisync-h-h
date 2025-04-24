using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UniSync.Areas.Identity.Data;
using System.Threading.Tasks;

namespace UniSync.Models
{
    public class NotLockedOutRequirement : IAuthorizationRequirement { }

    public class NotLockedOutHandler : AuthorizationHandler<NotLockedOutRequirement>
    {
        private readonly UserManager<UniSyncUser> _userManager;

        public NotLockedOutHandler(UserManager<UniSyncUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotLockedOutRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true) return;

            var user = await _userManager.GetUserAsync(context.User);
            if (user == null) return;

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}