using Microsoft.EntityFrameworkCore;

namespace TheWall.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) {}
        public DbSet<User> Users {get;set;}
        public DbSet<Message> Messages {get;set;}
        public DbSet<Comment> Comments {get;set;}
    }
}