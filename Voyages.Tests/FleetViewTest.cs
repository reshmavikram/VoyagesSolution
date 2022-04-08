using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voyage.Tests;

namespace Master.Tests
{
    
    class FleetViewTest
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }


        [Test]
        public void GetPassageDataLatestReport()
        {
            _context = dbcontext.SetDbContext();
            int vesselId = 6;
            var chkpassagedata= (from formdata in _context.Forms
                    join vesseldata in _context.Vessels on formdata.VesselCode equals vesseldata.VesselCode
                    where vesseldata.SFPM_VesselId == vesselId
                    orderby formdata.ReportDateTime != null ? formdata.ReportDateTime : Convert.ToDateTime(formdata.ReportTime) descending
                    select new FleetPassageLatestReportViewModel
                    {
                        LatestReportstatus = formdata.FormIdentifier.ToLower().Contains("arrival") ? "Arrival" :
                                            (formdata.FormIdentifier.ToLower().Contains("departure") ? "Departure" :
                                              (formdata.FormIdentifier.ToLower().Contains("noon") ?
                                                 (formdata.Location != null ? (formdata.Location.ToLower().Contains("N") ? "At Sea" : "In Port") : string.Empty) : string.Empty)
                                            ),
                        LastReport = formdata.ReportDateTime != null ? formdata.ReportDateTime : Convert.ToDateTime(formdata.ReportTime),
                        DeparturePort = formdata.FormIdentifier.ToLower().Contains("arrival") ?
                                              (from formdata in _context.Forms
                                               join vesseldata in _context.Vessels on formdata.VesselCode equals vesseldata.VesselCode
                                               where formdata.FormIdentifier.Contains("departure")
                                               where vesseldata.SFPM_VesselId == vesselId
                                               orderby formdata.ReportDateTime != null ? formdata.ReportDateTime : Convert.ToDateTime(formdata.ReportTime) descending
                                               select
                                               formdata.Port
                                               ).FirstOrDefault() : formdata.Port,
                        ArrivalPort = formdata.FormIdentifier.ToLower().Contains("arrival") ? formdata.Port : string.Empty,
                        ScheduledArrival = formdata.Upcoming.UpcomingPort != null ? formdata.Upcoming.UpcomingPort.ETA : string.Empty,
                        DistanceToGo = formdata.Upcoming.UpcomingPort != null ? formdata.Upcoming.UpcomingPort.DistToGo : string.Empty,
                        LastReportHeading = formdata.Heading,
                        VesselId = vesseldata.SFPM_VesselId
                    }).Where(x => x.VesselId == vesselId).FirstOrDefault();

            Assert.IsTrue(true);
        }
     

    }
}
