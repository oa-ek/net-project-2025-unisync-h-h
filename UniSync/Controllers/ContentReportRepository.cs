using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniSync.Data;
using UniSync.Models.Entity;

namespace UniSync.Repositories
{
    public class ContentReportRepository : IContentReportRepository
    {
        private readonly UniSyncContext _context;

        public ContentReportRepository(UniSyncContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ContentReport>> GetAllReportsAsync()
        {
            return await _context.ContentReports
                .Include(r => r.Reporter)
                .Include(r => r.Moderator)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContentReport>> GetPendingReportsAsync()
        {
            return await _context.ContentReports
                .Include(r => r.Reporter)
                .Where(r => r.Status == ReportStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ContentReport> GetReportByIdAsync(int id)
        {
            var report = await _context.ContentReports
                .Include(r => r.Reporter)
                .Include(r => r.Moderator)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report != null)
            {
                // Завантажуємо пов'язаний контент в залежності від типу
                switch (report.ContentType)
                {
                    case ContentType.News:
                        report.ReportedNews = await _context.News.FindAsync(report.ContentId);
                        break;
                        // Додайте інші типи контенту за необхідності
                }
            }

            return report;
        }

        public async Task<IEnumerable<ContentReport>> GetReportsByContentAsync(ContentType contentType, int contentId)
        {
            return await _context.ContentReports
                .Include(r => r.Reporter)
                .Include(r => r.Moderator)
                .Where(r => r.ContentType == contentType && r.ContentId == contentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContentReport>> GetReportsByUserAsync(string userId)
        {
            return await _context.ContentReports
                .Include(r => r.Reporter)
                .Include(r => r.Moderator)
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddReportAsync(ContentReport report)
        {
            await _context.ContentReports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(ContentReport report)
        {
            _context.ContentReports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(int id)
        {
            var report = await _context.ContentReports.FindAsync(id);
            if (report != null)
            {
                _context.ContentReports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPendingReportsCountAsync()
        {
            return await _context.ContentReports
                .CountAsync(r => r.Status == ReportStatus.Pending);
        }
    }
}