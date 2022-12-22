using Microsoft.EntityFrameworkCore;

namespace TwitterWatcher.Models;

public class DatabaseContext : DbContext
{
    public DbSet<Artist> Artists { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(Config.PsqlConnectionString)
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Artist>().Ignore(x => x.MediaTweets);
    }
}