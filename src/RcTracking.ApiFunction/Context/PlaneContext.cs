using Microsoft.EntityFrameworkCore;
using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction.Context
{
    public class PlaneContext : DbContext
    {
        public PlaneContext(DbContextOptions<PlaneContext> options) : base(options) { }

        public DbSet<PlaneModel> Planes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the entity to map to the Cosmos DB container
            modelBuilder.Entity<PlaneModel>().ToContainer("planes");
            modelBuilder.Entity<PlaneModel>().HasPartitionKey(c => c.Id);
        }
    }
}
