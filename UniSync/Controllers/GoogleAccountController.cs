using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UniSync.Controllers
{
    public class GoogleAccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public GoogleAccountController(
            SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        /*
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            var redirectUrl = Url.Page(
                "/Account/ExternalLogin", // Вказуємо на стандартну сторінку Identity UI
                pageHandler: "Callback",
                values: new { returnUrl },
                protocol: "https");

            var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Ця частина була для вирішення конкретних проблем з prompt, але часто не потрібна.
            if (props.Parameters.ContainsKey("prompt"))
            {
                props.Parameters.Remove("prompt");
            }

            return Challenge(props, provider);
        }
        */

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}