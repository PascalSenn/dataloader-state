using Microsoft.EntityFrameworkCore;

namespace DataLoader.State.Example;

public class BloggingContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}