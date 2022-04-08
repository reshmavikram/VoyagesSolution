using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using System.Collections.Generic;
using VoyagesAPIService.Infrastructure.Repositories;
using VoyagesAPIService.Infrastructure.Services.Interfaces;

namespace VoyagesAPIService.Infrastructure.Services
{
    public class EmsService : IEmsService
    {
        protected EmsRepository _voyagesrepository;
   /*     public VoyagesService(DatabaseContext databaseContext, UserContext context)
        {
            _voyagesrepository = new VoyagesRepository(databaseContext, context);
        }*/
        public EmsService(DatabaseContext databaseContext, UserContext context)
        {
            _voyagesrepository = new EmsRepository(databaseContext, context);
        }

        public IEnumerable<Voyages> GetAllVoyagesByVessel(string imoNumber, long userId)
        {
            return _voyagesrepository.GetAllVoyagesByVessel(imoNumber, userId);
        }
        public PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId)
        {
            return _voyagesrepository.GetLatestPassagesApprovalAudit(voyagesId);
        }

        public PageSettings GetPageSettings(long userId)
        {
            return _voyagesrepository.GetPageSettings(userId);
        }

        public IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId, long loginUserId)
        {
            return _voyagesrepository.GetPassageReportExclusionlog(voyageId, loginUserId);
        }

        public IEnumerable<PassageWarning> GetPassageWarning(long voyageId, long loginUserId)
        {
            return _voyagesrepository.GetPassageWarning(voyageId, loginUserId);
        }

        public IEnumerable<PassageWarningAudit> GetPassageWarningAudit(long PassageWarningId)
        {
            return _voyagesrepository.GetPassageWarningAudit(PassageWarningId);
        }
    }
}
