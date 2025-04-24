using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using UniSync.Areas.Identity.Data;
using UniSync.Models.Entity;
using UniSync.Constants;
using UniSync.Data;
using UniSync.Models;
using UniSync.Services;

var builder = WebApplication.CreateBuilder(args);

// Додаємо DbContext
builder.Services.AddDbContext<UniSyncContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Налаштування Identity
builder.Services.AddIdentity<UniSyncUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Налаштування блокування облікових записів
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<UniSyncContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Налаштування автентифікаційного cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToAccessDenied = context =>
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UniSyncUser>>();
                var user = userManager.GetUserAsync(context.HttpContext.User).Result;

                if (user != null && user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    context.Response.Redirect("/AccountStatus/Locked");
                    return Task.CompletedTask;
                }
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

// Додаємо Google аутентифікацію
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                               ?? throw new InvalidOperationException("Google ClientSecret is not configured.");
        options.CallbackPath = "/signin-google";
    });

builder.Services.AddTransient<IEmailSender, DummyEmailSender>();
builder.Services.AddScoped<RoleService>();

// Налаштування авторизації
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireStudent", policy =>
        policy.RequireRole(Roles.Student, Roles.Moderator, Roles.CourseManager, Roles.NewsEditor, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("RequireModerator", policy =>
        policy.RequireRole(Roles.Moderator, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("RequireCourseManager", policy =>
        policy.RequireRole(Roles.CourseManager, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("RequireNewsEditor", policy =>
        policy.RequireRole(Roles.NewsEditor, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole(Roles.SuperAdmin));

    options.AddPolicy("NotLockedOut", policy =>
        policy.AddRequirements(new NotLockedOutRequirement()));

    // Додаємо політику NotLockedOut як глобальну вимогу
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new NotLockedOutRequirement())
        .Build();
});

// Додаємо MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IAuthorizationHandler, NotLockedOutHandler>();

var app = builder.Build();

// Конфігурація HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ініціалізація ролей та супер-адміна
await using (var scope = app.Services.CreateAsyncScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UniSyncUser>>();

    // Створення ролей
    foreach (var role in Roles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Створення супер-адміна
    const string superEmail = "superadmin@unisync.com";
    var superAdmin = await userManager.FindByEmailAsync(superEmail);
    if (superAdmin == null)
    {
        superAdmin = new UniSyncUser
        {
            UserName = superEmail,
            Email = superEmail,
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin",
            LockoutEnabled = false,
            Articles = new List<Article>()
        };
        var result = await userManager.CreateAsync(superAdmin, "AdminPassword38060798$34@76#");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
    }
}

app.Run();

// Допоміжні класи
public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"Email to {email}: {subject}");
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}