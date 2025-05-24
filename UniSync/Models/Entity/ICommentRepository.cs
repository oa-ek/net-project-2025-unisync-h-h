using Microsoft.EntityFrameworkCore;
using UniSync.Data;

namespace UniSync.Models.Entity
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<Comment> GetCommentByIdAsync(int id);
    }

    public class CommentRepository : ICommentRepository
    {
        private readonly UniSyncContext _context;

        public CommentRepository(UniSyncContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }
    }
}