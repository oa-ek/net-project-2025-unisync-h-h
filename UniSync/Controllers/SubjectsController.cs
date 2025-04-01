using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniSync.Areas.Identity.Data;
using UniSync.Data;
using UniSync.Models.Entity;

namespace UniSync.Controllers
{
    [Authorize]
    public class SubjectsController : Controller
    {
        private readonly UniSyncContext _context;
        private readonly UserManager<UniSyncUser> _userManager;

        public SubjectsController(UniSyncContext context, UserManager<UniSyncUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Subjects
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var subjects = await _context.Subjects
                .Where(s => s.UserId == currentUser.Id)
                .ToListAsync();

            return View(subjects);
        }

        // GET: Subjects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                subject.UserId = currentUser.Id;

                _context.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Subject subject)
        {
            if (id != subject.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var existingSubject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (existingSubject == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingSubject.Title = subject.Title;
                    _context.Update(existingSubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == currentUser.Id);

            if (subject == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}