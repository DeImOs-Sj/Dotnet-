using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FormData> FormDatas { get; set; }
}
