using KzBarry.Data;
using KzBarry.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KzBarry.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(DataContext context) : base(context) { }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _dbSet.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }
        public async Task<int> DeleteExpiredAsync()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _dbSet.Where(r => r.Expires < now);
            int count = await expiredTokens.CountAsync();
            if (count > 0)
            {
                _dbSet.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
            return count;
        }
    }
}
