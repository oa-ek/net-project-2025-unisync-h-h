using System.Collections.Generic;
using System.Threading.Tasks;
using UniSync.Models.Entity;

namespace UniSync.Repositories
{
    public interface IContentReportRepository
    {
        Task<IEnumerable<ContentReport>> GetAllReportsAsync();
        Task<IEnumerable<ContentReport>> GetPendingReportsAsync();
        Task<ContentReport> GetReportByIdAsync(int id);
        Task<IEnumerable<ContentReport>> GetReportsByContentAsync(ContentType contentType, int contentId);
        Task<IEnumerable<ContentReport>> GetReportsByUserAsync(string userId);
        Task AddReportAsync(ContentReport report);
        Task UpdateReportAsync(ContentReport report);
        Task DeleteReportAsync(int id);
        Task<int> GetPendingReportsCountAsync();
    }
}