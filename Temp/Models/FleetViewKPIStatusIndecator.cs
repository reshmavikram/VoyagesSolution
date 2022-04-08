using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    [Serializable]
    public class FleetViewKPIStatusIndicator
    {
        public long VesselGroupId { get; set; }
        public long VesselId { get; set; }
        public string VesselName { get; set; }
        public string VesselStatus { get; set; }
        public string NextPort { get; set; }
        public string Weathercolor { get; set; }
       // public string SludgeStatuscolor { get; set; }
       // public string Bilgestatuscolor { get; set; }
        public string Speedcolor { get; set; }
        public string ConsumptionME { get; set; }
        public string Consumptionaux { get; set; }
        public string PortAtSeaColor { get; set; }
       // public string ScheduleColor { get; set; }
       // public string EcaNoEcaColor { get; set; }
        public string IMONumber { get; set; }
    }
}
