using Microsoft.EntityFrameworkCore;

namespace BarfSourceName.Infrastructure.Database;

public class DatabaseContext : DbContext
{

    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}
