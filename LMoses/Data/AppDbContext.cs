using LMoses.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LMoses.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClickCounter> ClickCounters { get; set; }
    }
}
