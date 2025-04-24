using Microsoft.EntityFrameworkCore;
using UniSync.Data;
using UniSync.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using UniSync.Constants;
using UniSync.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UniSyncContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<UniSyncUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<UniSyncContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddAuthentication()
    .AddGoogle(google =>
    {
        google.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        google.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        google.CallbackPath = "/signin-google";
    });

builder.Services.AddTransient<IEmailSender, DummyEmailSender>();
builder.Services.AddScoped<RoleService>();

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
});

builder.Services.AddRazorPages();

var app = builder.Build();

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

    const string superEmail = "superadmin@unisync.com";
    var super = await userManager.FindByEmailAsync(superEmail);
    if (super == null)
    {
        super = new UniSyncUser
        {
            UserName = superEmail,
            Email = superEmail,
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin"
        };
        var res = await userManager.CreateAsync(super, "SuperAdmin123!");
        if (res.Succeeded)
            await userManager.AddToRoleAsync(super, Roles.SuperAdmin);
    }
}

app.Run();

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"Email to {email}: {subject}");
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}
