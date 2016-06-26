namespace RealtimeDashboardApp.Entity
{
    class FuelEfficientDrivingModelRecord
    {
        public int Id { get; set; }
        public string vin { get; set; }
        public string model { get; set; }
        
        public string city { get; set; }
        public string transmission_gear_position { get; set; }
        public int speed { get; set; }
        public int brake_pedal_status { get; set; }
        public int accelerator_pedal_position { get; set; }
        
        public int Year { get; set; }
        public int Month { get; set; }       
       
    }
}
