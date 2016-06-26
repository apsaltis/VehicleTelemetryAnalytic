using System;

namespace RealtimeDashboardApp.Entity
{

    public class RealTimeVehicleHealthAnomalyReport
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string vin { get; set; }
        public string Model { get; set; }
        public string timestamp { get; set; }
        public int outsideTemperature { get; set; }
        public int engineTemperature { get; set; }
        public int speed { get; set; }
        public int fuel { get; set; }
        public int engineoil { get; set; }
        public int tirepressure { get; set; }
        public int odometer { get; set; }
        public string city { get; set; }
        public int accelerator_pedal_position { get; set; }
        public bool parking_brake_status { get; set; }
        public bool headlamp_status { get; set; }
        public bool brake_pedal_status { get; set; }
        public string transmission_gear_position { get; set; }
        public bool ignition_status { get; set; }
        public bool windshield_wiper_status { get; set; }
        public bool abs { get; set; }

        public int MaintenanceLabel { get; set; }
        public Double MaintenanceProbability { get; set; }

        public int RecallLabel { get; set; }
        public Double RecallProbability { get; set; }
    }
}