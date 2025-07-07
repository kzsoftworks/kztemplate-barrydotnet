using KzBarry.Data;
using KzBarry.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace KzBarry.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DataContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email is required.");

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        }
    }
}
