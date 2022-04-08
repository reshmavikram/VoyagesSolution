using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace VoyagesAPIService.Infrastructure.Services.Interfaces
{
    public interface IEmsService
    {
        IEnumerable<Voyages> GetAllVoyagesByVessel(string imoNumber, long userId);
        PageSettings GetPageSettings(long userId);
        IEnumerable<PassageWarning> GetPassageWarning(long voyageId, long loginUserId);
        IEnumerable<PassageWarningAudit> GetPassageWarningAudit(long PassageWarningId);
        PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId);
        IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId, long loginUserId);
    }
}
