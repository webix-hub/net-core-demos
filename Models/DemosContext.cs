using Microsoft.EntityFrameworkCore;

namespace WebixDemos
{
    public class DemosContext : DbContext
    {
        public DbSet<Sheet> Sheets { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Person> Persons { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=demos.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}