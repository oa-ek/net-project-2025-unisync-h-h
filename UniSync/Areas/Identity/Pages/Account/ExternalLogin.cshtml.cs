using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using UniSync.Areas.Identity.Data;

namespace UniSync.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<UniSyncUser> _signInManager;
        private readonly UserManager<UniSyncUser> _userManager;
        private readonly IUserStore<UniSyncUser> _userStore;
        private readonly IUserEmailStore<UniSyncUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<UniSyncUser> signInManager,
            UserManager<UniSyncUser> userManager,
            IUserStore<UniSyncUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Поле 'Електронна пошта' є обов'язковим.")] // Додано повідомлення про помилку
            [EmailAddress(ErrorMessage = "Невірний формат електронної пошти.")] // Додано повідомлення про помилку
            public string Email { get; set; }

            [Required(ErrorMessage = "Поле 'Ім'я' є обов'язковим.")] // ДОДАНО: Валідація для імені
            [Display(Name = "Ім'я")] // ДОДАНО: Відображуване ім'я для поля
            public string FirstName { get; set; } // ДОДАНО: Властивість для імені

            [Required(ErrorMessage = "Поле 'Прізвище' є обов'язковим.")] // ДОДАНО: Валідація для прізвища
            [Display(Name = "Прізвище")] // ДОДАНО: Відображуване ім'я для поля
            public string LastName { get; set; } // ДОДАНО: Властивість для прізвища
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Помилка від зовнішнього провайдера: {remoteError}"; // Переклад
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Помилка завантаження інформації про зовнішній вхід."; // Переклад
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;

                // ДОДАНО: Спроба отримати ім'я та прізвище з клеймів Google
                string email = info.Principal.FindFirstValue(ClaimTypes.Email);
                string firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName); // ClaimTypes.GivenName для імені
                string lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);   // ClaimTypes.Surname для прізвища

                Input = new InputModel
                {
                    Email = email,
                    FirstName = firstName, // ДОДАНО: Передача імені з клеймів
                    LastName = lastName    // ДОДАНО: Передача прізвища з клеймів
                };

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Помилка завантаження інформації про зовнішній вхід під час підтвердження."; // Переклад
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Встановлення імені користувача та електронної пошти
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // ДОДАНО: Встановлення імені та прізвища для UniSyncUser
                // Переконайтеся, що ваш UniSyncUser має властивості FirstName та LastName
                if (user is UniSyncUser uniSyncUser)
                {
                    uniSyncUser.FirstName = Input.FirstName;
                    uniSyncUser.LastName = Input.LastName;
                }

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Користувач створив обліковий запис за допомогою провайдера {Name}.", info.LoginProvider); // Переклад

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Підтвердіть вашу електронну пошту", // Переклад
                            $"Будь ласка, підтвердіть ваш обліковий запис, <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>натиснувши тут</a>."); // Переклад

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private UniSyncUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UniSyncUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Неможливо створити екземпляр '{nameof(UniSyncUser)}'. " + // Переклад
                    $"Переконайтеся, що '{nameof(UniSyncUser)}' не є абстрактним класом і має конструктор без параметрів, або ж " + // Переклад
                    $"перевизначте сторінку зовнішнього входу в /Areas/Identity/Pages/Account/ExternalLogin.cshtml"); // Переклад
            }
        }

        private IUserEmailStore<UniSyncUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("UI за замовчуванням вимагає сховища користувачів з підтримкою електронної пошти."); // Переклад
            }
            return (IUserEmailStore<UniSyncUser>)_userStore;
        }
    }
}
