using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSync.Data; // Використовуємо UniSyncContext
using UniSync.Models;

namespace UniSync.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UniSyncContext _context; // Замінюємо ApplicationDbContext на UniSyncContext

    public HomeController(ILogger<HomeController> logger, UniSyncContext context) // Оновлюємо конструктор
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects
            .Include(p => p.Subject)
            .ToListAsync();

        return View(projects);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}