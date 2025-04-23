using Microsoft.AspNetCore.Identity;
using UniSync.Areas.Identity.Data;
using UniSync.Constants;

namespace UniSync.Services
{
    public class RoleService
    {
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(UserManager<UniSyncUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task EnsureRolesCreatedAsync()
        {
            foreach (var roleName in Roles.AllRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return false;
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                return result.Succeeded;
            }

            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            }

            return true;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> BanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return result.Succeeded;
        }

        public async Task<bool> UnbanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }
    }
}