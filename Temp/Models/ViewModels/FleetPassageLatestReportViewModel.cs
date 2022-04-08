using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class FleetPassageLatestReportViewModel
    {
        public string LatestReportstatus { get; set; }
        public DateTimeOffset? LastReport { get; set; }
        public string DeparturePort { get; set; }
        public string ArrivalPort { get; set; }
        public string ScheduledArrival { get; set; }
        public string DistanceToGo { get; set; }
        public string LastReportHeading { get; set; }
        public long VesselId { get; set; }
    }
}
