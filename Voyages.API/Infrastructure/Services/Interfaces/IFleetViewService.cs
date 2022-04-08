using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoyagesAPIService.Infrastructure.Services.Interfaces
{
    public interface IFleetViewService
    {
        IEnumerable<FleetViewKPIStatusIndicator> GetFleetView(long vesselGroupId,long loginUserId);
        FleetPassageLatestReportViewModel GetPassageDataLatestReport(long vesselId,long loginUserId);
        List<FleetPassageDataViewModel> GetPassageData(long vesselId,long loginUserId);
    }
}
