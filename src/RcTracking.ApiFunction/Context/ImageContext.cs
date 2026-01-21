using Microsoft.EntityFrameworkCore;
using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction.Context
{
    public class ImageContext : DbContext
    {
        public ImageContext(DbContextOptions<ImageContext> options) : base(options) { }

        public DbSet<ImageModel> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the entity to map to the Cosmos DB container
            modelBuilder.Entity<ImageModel>().ToContainer("images");
            modelBuilder.Entity<ImageModel>().HasPartitionKey(c => c.Id);
        }
    }
}
