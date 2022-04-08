using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using VoyagesAPIService.Infrastructure.Repositories;
using VoyagesAPIService.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoyagesAPIService.Infrastructure.Services
{
    public class FleetViewServices : IFleetViewService
    {
        protected FleetViewRepository _repository;

        public FleetViewServices(DatabaseContext databaseContext,UserContext currentUser)
        {
            _repository = new FleetViewRepository(databaseContext, currentUser);
        }

        public IEnumerable<FleetViewKPIStatusIndicator> GetFleetView(long vesselGroupId,long loginUserId)
        {
            return _repository.GetFleetView(vesselGroupId, loginUserId);
        }
        public List<FleetPassageDataViewModel> GetPassageData(long vesselId,long loginUserId)
        {
            return _repository.GetPassageData(vesselId, loginUserId);
        }

        public FleetPassageLatestReportViewModel GetPassageDataLatestReport(long vesselId,long loginUserId)
        {
            return _repository.GetPassageDataLatestReport(vesselId, loginUserId);
        }
    }
}
