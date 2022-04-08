using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    [Serializable]
    public class FleetPassageDataViewModel
    {
        public int? Voyage { get; set; }
        public DateTimeOffset? DepartureDate { get; set; }
        public string DeparturePort { get; set; }
        public DateTimeOffset? ArrivalDate { get; set; }
        public string ArrivalPort { get; set; }
        public long VesselId { get; set; }
        public long formId { get; set; }
    }
}
