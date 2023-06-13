namespace mAIa.Data
{
    using mAIa.Data.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class mAIaDataContext : DbContext
    {
        public mAIaDataContext(DbContextOptions<mAIaDataContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserTrait> UserTraits { get; set; }
    }
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<mAIaDataContext>
    {
        public mAIaDataContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<mAIaDataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new mAIaDataContext(optionsBuilder.Options);
        }
    }
}
