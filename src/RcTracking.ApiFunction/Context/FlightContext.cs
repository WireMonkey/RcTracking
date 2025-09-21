using Microsoft.EntityFrameworkCore;
using RcTracking.ApiFunction.Model;

namespace RcTracking.ApiFunction.Context
{
    public class FlightContext : DbContext
    {
        public FlightContext(DbContextOptions<FlightContext> options) : base(options) { }

        public DbSet<FlightModel> Flights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the entity to map to the Cosmos DB container
            modelBuilder.Entity<FlightModel>().ToContainer("flights");
            modelBuilder.Entity<FlightModel>().HasPartitionKey(c => c.Id);
        }
    }
}
