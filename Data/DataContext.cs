using KzBarry.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace KzBarry.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public override int SaveChanges()
        {
            ApplyAuditing();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditing();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Handles CreatedAt and UpdatedAt modifications
        /// </summary>
        private void ApplyAuditing()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is IAuditable &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var auditable = (IAuditable)entry.Entity;

                auditable.UpdatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                    auditable.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
