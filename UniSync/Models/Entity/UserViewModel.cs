using Microsoft.AspNetCore.Identity;

namespace UniSync.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public bool IsLocked { get; set; }
    }

    public class RoleViewModel
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleViewModel> Roles { get; set; }
    }
    public class IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}