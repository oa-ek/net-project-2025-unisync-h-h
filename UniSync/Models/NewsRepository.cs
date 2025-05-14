using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniSync.Data;
using UniSync.Models.Entity;

namespace UniSync.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly UniSyncContext _context;

        public NewsRepository(UniSyncContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync()
        {
            return await _context.News
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetLatestNewsAsync(int count = 5)
        {
            return await _context.News
                .OrderByDescending(n => n.PublishedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task<IEnumerable<News>> GetNewsByCategoryAsync(NewsCategory category)
        {
            return await _context.News
                .Where(n => n.Category == category)
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();
        }

        public async Task AddNewsAsync(News news)
        {
            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNewsAsync(News news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNewsAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
        }
    }
}