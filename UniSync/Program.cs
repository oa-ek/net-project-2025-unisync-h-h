using Microsoft.EntityFrameworkCore;
using UniSync.Data;
using UniSync.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ������ ������
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // ϳ������� Razor Pages ��� Identity

// ������ DbContext
builder.Services.AddDbContext<UniSyncContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ������ Identity
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

// ������������ HTTP pipeline
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

app.MapRazorPages(); // �������� ��� Razor Pages (Identity)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();