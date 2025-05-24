using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSync.Data;
using UniSync.Models;

namespace UniSync.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UniSyncContext _context;

    public HomeController(ILogger<HomeController> logger, UniSyncContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string filter = "all", string sort = "deadline_asc", string searchString = null)
    {
        var projects = _context.Projects.Include(p => p.Subject).AsQueryable();

        // Застосування пошуку
        if (!string.IsNullOrEmpty(searchString))
        {
            projects = projects.Where(p => p.Title.Contains(searchString));
        }

        // Застосування фільтра
        if (!string.IsNullOrEmpty(filter))
        {
            switch (filter.ToLower())
            {
                case "high":
                    projects = projects.Where(p => p.Priority == "High");
                    break;
                case "due_soon":
                    projects = projects.Where(p => p.Deadline <= DateTime.Now.AddDays(7) && p.Status != "Completed");
                    break;
                case "completed":
                    projects = projects.Where(p => p.Status == "Completed");
                    break;
            }
        }

        // Застосування сортування
        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "deadline_asc":
                    projects = projects.OrderBy(p => p.Deadline);
                    break;
                case "deadline_desc":
                    projects = projects.OrderByDescending(p => p.Deadline);
                    break;
                case "priority_asc":
                    projects = projects.OrderBy(p => MapPriorityToNumber(p.Priority));
                    break;
                case "priority_desc":
                    projects = projects.OrderByDescending(p => MapPriorityToNumber(p.Priority));
                    break;
                case "progress_asc":
                    projects = projects.OrderBy(p => p.Progress);
                    break;
                case "progress_desc":
                    projects = projects.OrderByDescending(p => p.Progress);
                    break;
                case "title_asc":
                    projects = projects.OrderBy(p => p.Title);
                    break;
                case "title_desc":
                    projects = projects.OrderByDescending(p => p.Title);
                    break;
                default:
                    projects = projects.OrderBy(p => p.Deadline);
                    break;
            }
        }
        else
        {
            projects = projects.OrderBy(p => p.Deadline);
        }

        var projectList = await projects.ToListAsync();

        ViewBag.CurrentFilter = filter;
        ViewBag.CurrentSort = sort;
        ViewBag.SearchString = searchString;

        return View(projectList);
    }

    private int MapPriorityToNumber(string priority)
    {
        switch (priority.ToLower())
        {
            case "high": return 3;
            case "medium": return 2;
            case "low": return 1;
            default: return 0;
        }
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