using System.Collections.Generic;
using System.Threading.Tasks;
using UniSync.Models.Entity;

namespace UniSync.Repositories
{
    public interface INewsRepository
    {
        Task<IEnumerable<News>> GetAllNewsAsync();
        Task<IEnumerable<News>> GetLatestNewsAsync(int count = 5);
        Task<News> GetNewsByIdAsync(int id);
        Task<IEnumerable<News>> GetNewsByCategoryAsync(NewsCategory category);
        Task AddNewsAsync(News news);
        Task UpdateNewsAsync(News news);
        Task DeleteNewsAsync(int id);
    }
}