using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UygApi.Models;


namespace UygApi.Repositories
{
    public class StarredRepository
    {
        private readonly AppDbContext _context;
        public StarredRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Starred> AddStarredAsync(Starred starred)
        {
            await _context.Starreds.AddAsync(starred);
            await _context.SaveChangesAsync();
            return starred;
        }
        public async Task<IEnumerable<Starred>> GetStarredsByUserIdAsync(string userId)
        {
            return await _context.Starreds
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }
        public async Task<bool> RemoveStarredAsync(int starredId)
        {
            var starred = await _context.Starreds.FindAsync(starredId);
            if (starred == null) return false;
            _context.Starreds.Remove(starred);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
