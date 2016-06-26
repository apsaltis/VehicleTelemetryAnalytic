using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RealtimeDashboardApp.Entity
{
    [DataContract]
    class DatasetShema
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public Dictionary<string, string> tables;

        public DatasetShema(string DatasetName)
        {
            name = DatasetName;
            tables = new Dictionary<string, string>();
            tables.Add("AggressiveDrivingRecord01", JsonHelper.JsonSerializer<AggressiveDrivingRecord>(new AggressiveDrivingRecord()));
            //tables.Add("FuelEfficientDrivingModelRecord", new FuelEfficientDrivingModelRecord());
            //tables.Add("RealTimeVehicleHealthRecord", new RealTimeVehicleHealthRecord());
            //tables.Add("RecallModelRecord", new RecallModelRecord());
        }
    }
}
