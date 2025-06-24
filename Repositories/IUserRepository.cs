using KzBarry.Models.Entities;

namespace KzBarry.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
    }
}
