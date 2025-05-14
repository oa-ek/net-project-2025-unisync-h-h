using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UniSync.Areas.Identity.Data;
using System.Threading.Tasks;

public class NotLockedOutRequirement : IAuthorizationRequirement { }

public class NotLockedOutHandler : AuthorizationHandler<NotLockedOutRequirement>
{
    private readonly UserManager<UniSyncUser> _userManager;

    public NotLockedOutHandler(UserManager<UniSyncUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotLockedOutRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Succeed(requirement);
            return;
        }

        var user = await _userManager.GetUserAsync(context.User);
        if (user == null)
        {
            context.Fail();
            return;
        }

        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        if (!isLockedOut)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}