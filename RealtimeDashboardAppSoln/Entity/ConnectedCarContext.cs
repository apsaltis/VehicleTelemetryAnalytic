using System.Data.Entity;

namespace RealtimeDashboardApp.Entity
{
    class ConnectedCarContext : DbContext
    {
        //public DbSet<AggressiveDrivingRecord> AggressiveDrivingRecords { get; set; }
        //public DbSet<FuelEfficientDrivingModelRecord> FuelEfficientDrivingModelRecords { get; set; }
        //public DbSet<RealTimeVehicleHealthRecord> RealTimeVehicleHealthRecords { get; set; }
        //public DbSet<RecallModelRecord> RecallModelRecords { get; set; }

        public DbSet<RealTimeVehicleHealthAnomalyReport> VehicleHealthReportMls { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<AggressiveDrivingRecord>().ToTable("AggresiveDrivingModelReport");
            //modelBuilder.Entity<FuelEfficientDrivingModelRecord>().ToTable("FuelEfficientDrivingModelReport");
            //modelBuilder.Entity<RealTimeVehicleHealthRecord>().ToTable("RealTimeVehicleHealthReport");
            //modelBuilder.Entity<RecallModelRecord>().ToTable("RecallModelReport");
        } 
    }
}
