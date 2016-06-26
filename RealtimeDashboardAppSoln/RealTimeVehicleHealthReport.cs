using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealtimeDashboardApp
{
    [Table("RealTimeVehicleHealthReport")]
    public class RealTimeVehicleHealthReport
    {
        public int Id { get; set; }

        public string Type { get; set; }

        [StringLength(256)]
        public string model { get; set; }

        [StringLength(256)]
        public string city { get; set; }

        [StringLength(256)]
        public string cars { get; set; }

        public decimal? engineTemperature { get; set; }

        public decimal? Speed { get; set; }

        public decimal? Fuel { get; set; }

        public decimal? EngineOil { get; set; }

        public decimal? TirePressure { get; set; }

        public decimal? Odometer { get; set; }
    }

    public class TypeClass
    {
        public string Type { get; set; }
    }
}
