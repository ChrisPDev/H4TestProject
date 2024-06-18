using Microsoft.EntityFrameworkCore;
using H4TestProjectAPI.Models;

namespace H4TestProjectAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration configuration;

        public ApplicationDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<Person> Persons { get; set; }
    }
}
