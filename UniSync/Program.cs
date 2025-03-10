using Microsoft.EntityFrameworkCore;
using UniSync.Data;
using UniSync.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіси
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Підтримка Razor Pages для Identity

// Додаємо DbContext
builder.Services.AddDbContext<UniSyncContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Додаємо Identity
builder.Services.AddDefaultIdentity<UniSyncUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<UniSyncContext>();

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

app.MapRazorPages(); // Маршрути для Razor Pages (Identity)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();