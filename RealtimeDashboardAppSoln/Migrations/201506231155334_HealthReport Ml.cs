using System.Data.Entity.Migrations;

namespace RealtimeDashboardApp.Migrations
{
    public partial class HealthReportMl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VehicleHealthReportMls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        vin = c.String(),
                        Model = c.String(),
                        timestamp = c.String(),
                        outsideTemperature = c.String(),
                        engineTemperature = c.String(),
                        speed = c.String(),
                        fuel = c.String(),
                        engineoil = c.String(),
                        tirepressure = c.String(),
                        odometer = c.String(),
                        city = c.String(),
                        accelerator_pedal_position = c.String(),
                        parking_brake_status = c.String(),
                        headlamp_status = c.String(),
                        brake_pedal_status = c.String(),
                        transmission_gear_position = c.String(),
                        ignition_status = c.String(),
                        windshield_wiper_status = c.String(),
                        abs = c.String(),
                        MaintenanceLabel = c.Int(nullable: false),
                        MaintenanceProbability = c.Double(nullable: false),
                        RecallLabel = c.Int(nullable: false),
                        RecallProbability = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VehicleHealthReportMls");
        }
    }
}
