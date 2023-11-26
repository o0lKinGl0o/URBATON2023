using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebApplication3;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<ApplicationUser> users { get; set; }
    public DbSet<InvalidToken> InvalidTokens { get; set; }
    public DbSet<Post> post { get; set; }
}