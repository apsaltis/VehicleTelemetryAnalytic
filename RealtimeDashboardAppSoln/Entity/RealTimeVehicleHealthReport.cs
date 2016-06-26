namespace RealtimeDashboardApp.Entity
{
    class RealTimeVehicleHealthReport
    {
        public int Id { get; set; }
        
        public string model { get; set; }
        
        public string city { get; set; }
        public string cars { get; set; }

        public decimal engineTemperature { get; set; }
        public decimal Speed { get; set; }
        public decimal Fuel { get; set; }
        public decimal EngineOil { get; set; }
        public decimal TirePressure { get; set; }
        public decimal Odometer { get; set; }
    }
}
