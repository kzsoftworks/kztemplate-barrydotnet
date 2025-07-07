using KzBarry.Models.Entities;
using System.Threading.Tasks;

namespace KzBarry.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<int> DeleteExpiredAsync();
    }
}
